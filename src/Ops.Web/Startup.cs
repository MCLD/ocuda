using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Authorization;
using Ocuda.Ops.Controllers.RouteConstraints;
using Ocuda.Ops.Controllers.Validators;
using Ocuda.Ops.Data;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Web.StartupHelper;
using Ocuda.Utility.Keys;
using StackExchange.Redis;

namespace Ocuda.Ops.Web
{
    public class Startup
    {
        private const string DefaultCulture = "en-US";
        private const string CacheInstanceInternal = "ocuda.internal.ops";
        private const string DataProtectionKeyKey = "dpk";

        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // set a default culture of en-US if none is specified
            string culture = _config[Utility.Keys.Configuration.OpsCulture] ?? DefaultCulture;
            _logger.LogInformation("Configuring for culture: {0}", culture);
            services.Configure<RequestLocalizationOptions>(_ =>
            {
                _.DefaultRequestCulture
                    = new Microsoft.AspNetCore.Localization.RequestCulture(culture);
            });

            switch (_config[Utility.Keys.Configuration.OpsDistributedCache])
            {
                case "Redis":
                    string redisConfiguration
                        = _config[Utility.Keys.Configuration.OpsDistributedCacheRedisConfiguration]
                        ?? throw new Exception($"{Utility.Keys.Configuration.OpsDistributedCache} has Redis selected but {Utility.Keys.Configuration.OpsDistributedCacheRedisConfiguration} is not set.");
                    string instanceName = Utility.Keys.CacheInstance.OcudaOps;
                    string cacheDiscriminator
                        = _config[Utility.Keys.Configuration.OpsDistributedCacheInstanceDiscriminator]
                        ?? string.Empty;
                    if (!string.IsNullOrEmpty(cacheDiscriminator))
                    {
                        instanceName = $"{instanceName}.{cacheDiscriminator}";
                    }
                    _logger.LogInformation("Using Redis distributed cache {0} instance {1}", 
                        redisConfiguration, 
                        instanceName);
                    services.AddDistributedRedisCache(_ =>
                    {
                        _.Configuration = redisConfiguration;
                        _.InstanceName = instanceName;
                    });
                    var redis = ConnectionMultiplexer.Connect(redisConfiguration);
                    services.AddDataProtection()
                        .PersistKeysToRedis(redis, 
                            $"{CacheInstanceInternal}.{DataProtectionKeyKey}");
                    break;
                default:
                    _logger.LogInformation("Using memory-based distributed cache");
                    services.AddDistributedMemoryCache();
                    var sharedPath = string.Format("{0}{1}{2}",
                        Utility.Files.SharedPath.Get(_config[Utility.Keys.Configuration.OpsFileShared]),
                        System.IO.Path.DirectorySeparatorChar,
                        DataProtectionKeyKey);
                    services.AddDataProtection()
                        .PersistKeysToFileSystem(new System.IO.DirectoryInfo(sharedPath));
                    break;
            }

            string opsCs = _config.GetConnectionString("Ops")
                ?? throw new Exception("ConnectionString:Ops not configured.");
            string promCs = _config.GetConnectionString("Promenade")
                ?? throw new Exception("ConnectionString:Promenade not configured.");

            var provider = _config[Utility.Keys.Configuration.OpsDatabaseProvider];
            switch (provider)
            {
                case "SqlServer":
                    _logger.LogInformation("Using {0} data provider", provider);
                    services.AddDbContextPool<OpsContext,
                        DataProvider.SqlServer.Ops.Context>(_ => _.UseSqlServer(opsCs));
                    services.AddDbContextPool<PromenadeContext,
                        DataProvider.SqlServer.Promenade.Context>(_ => _.UseSqlServer(promCs));
                    break;
                case "SQLite":
                    _logger.LogInformation("Using {0} data provider", provider);
                    services.AddDbContextPool<OpsContext,
                        DataProvider.SQLite.Ops.Context>(_ => _.UseSqlite(opsCs));
                    services.AddDbContextPool<PromenadeContext,
                        DataProvider.SQLite.Promenade.Context>(_ => _.UseSqlite(promCs));
                    break;
                default:
                    _logger.LogCritical("No {0} configured in settings. Exiting.",
                        Utility.Keys.Configuration.OpsDatabaseProvider);
                    throw new Exception($"No {Utility.Keys.Configuration.OpsDatabaseProvider} configured.");
            }

            var sessionTimeout = TimeSpan.FromHours(2 * 60);
            if (int.TryParse(_config[Utility.Keys.Configuration.OpsSessionTimeoutMinutes],
                out int configuredTimeout))
            {
                _logger.LogInformation("Session timeout configured for {0} minutes",
                    configuredTimeout);
                sessionTimeout = TimeSpan.FromMinutes(configuredTimeout);
            }

            services.AddSession(_ =>
            {
                _.IdleTimeout = sessionTimeout;
                _.Cookie.HttpOnly = true;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(_ =>
                {
                    _.AccessDeniedPath = "/Home/Unauthorized";
                    _.LoginPath = "/Home/Authenticate";
                });

            services.AddSingleton<IAuthorizationHandler, SectionManagerHandler>();

            services.AddAuthorization(_ =>
            {
                _.AddPolicy(nameof(SectionManagerRequirement),
                    policy => policy.Requirements.Add(new SectionManagerRequirement()));
                _.AddPolicy(nameof(ClaimType.SiteManager),
                    policy => policy.RequireClaim(nameof(ClaimType.SiteManager)));
            });

            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1)
                .AddSessionStateTempDataProvider();

            // service facades
            services.AddScoped(typeof(Controllers.ServiceFacades.Controller<>));

            // filters
            services.AddScoped<Controllers.Filters.AuthenticationFilter>();
            services.AddScoped<Controllers.Filters.UserFilter>();
            services.AddScoped<Controllers.Filters.SectionFilter>();

            // section path validator
            services.AddScoped<ISectionPathValidator, SectionPathValidator>();

            // helpers
            services.AddScoped<Controllers.Helpers.LdapHelper>();
            services.AddScoped<Utility.Helpers.WebHelper>();

            // repositories
            services.AddScoped<Service.Interfaces.Ops.Repositories.ICategoryRepository,
                Data.Ops.CategoryRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IClaimGroupRepository,
                Data.Ops.ClaimGroupRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IFileRepository, 
                Data.Ops.FileRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IFileTypeRepository,
                Data.Ops.FileTypeRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ILinkRepository, 
                Data.Ops.LinkRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IPageRepository, 
                Data.Ops.PageRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IPostRepository, 
                Data.Ops.PostRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IRosterDetailRepository,
                Data.Ops.RosterDetailRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IRosterHeaderRepository,
                Data.Ops.RosterHeaderRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ISectionManagerGroupRepository,
                Data.Ops.SectionManagerGroupRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ISectionRepository,
                Data.Ops.SectionRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ISiteSettingRepository,
                Data.Ops.SiteSettingRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IUserRepository,
                Data.Ops.UserRepository>();

            // services
            services.AddScoped<Service.Interfaces.Ops.Services.IAuthorizationService, AuthorizationService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IFileTypeService, FileTypeService>();
            services.AddScoped<IInitialSetupService, InitialSetupService>();
            services.AddScoped<IInsertSampleDataService, InsertSampleDataService>();
            services.AddScoped<ILinkService, LinkService>();
            services.AddScoped<IPathResolverService, PathResolverService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IRosterService, RosterService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<ISiteSettingService, SiteSettingService>();
            services.AddScoped<IUserService, UserService>();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            IPathResolverService pathResolver)
        {
            // configure error page handling and development IDE linking
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/Index/{0}");
            }

            // insert remote address into the log context for each request
            app.Use(async (context, next) =>
            {
                using (Serilog.Context
                    .LogContext
                    .PushProperty("RemoteAddress", context.Connection.RemoteIpAddress))
                {
                    await next.Invoke();
                }
            });

            // update databases to include latest migrations
            app.InitialSetup();

            // use the culture configured above in services
            app.UseRequestLocalization();

            app.UseStaticFiles();

            // configure shared content directory
            var contentFilePath = pathResolver.GetPublicContentFilePath();
            var contentUrl = pathResolver.GetPublicContentUrl();
            if (!contentUrl.StartsWith("/"))
            {
                contentUrl = $"/{contentUrl}";
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider
                    = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(contentFilePath),
                RequestPath = new Microsoft.AspNetCore.Http.PathString(contentUrl)
            });

            app.UseSession();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: null,
                    template: "{area}/{section}/{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" },
                    constraints: new
                    {
                        section = new SectionRouteConstraint(app.ApplicationServices.GetRequiredService<ISectionPathValidator>())
                    });

                routes.MapRoute(
                    name: null,
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                   name: null,
                   template: "{section}/{controller}/{action}/{id?}",
                   defaults: new { controller = "Home", action = "Index" },
                   constraints: new
                   {
                       section = new SectionRouteConstraint(app.ApplicationServices.GetRequiredService<ISectionPathValidator>())
                   });

                routes.MapRoute(
                    name: null,
                    template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: null,
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
