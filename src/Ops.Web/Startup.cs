using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.RouteConstraint;
using Ocuda.Ops.Controllers.Validator;
using Ocuda.Ops.Data;
using Ocuda.Ops.Service;
using Ocuda.Ops.Web.StartupHelper;
using Ocuda.Utility.Web;

namespace Ocuda.Ops.Web
{
    public class Startup
    {
        private const string DefaultCulture = "en-US";

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
            _logger.LogInformation($"Configuring for culture: {culture}");
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
                    string instanceName = "Ocuda.Ops";
                    _logger.LogInformation($"Using Redis distributed cache {redisConfiguration} instance {instanceName}");
                    services.AddDistributedRedisCache(_ =>
                    {
                        _.Configuration = redisConfiguration;
                        _.InstanceName = instanceName;
                    });
                    break;
                default:
                    _logger.LogInformation("Using memory-based distributed cache");
                    services.AddDistributedMemoryCache();
                    break;
            }

            string opsCs = _config.GetConnectionString("Ops")
                ?? throw new Exception("ConnectionString:Ops not configured.");
            string promCs = _config.GetConnectionString("Promenade")
                ?? throw new Exception("ConnectionString:Promenade not configured.");

            switch (_config[Utility.Keys.Configuration.OpsDatabaseProvider])
            {
                case "SqlServer":
                    _logger.LogInformation("Using SqlServer data provider");
                    services.AddDbContextPool<OpsContext, 
                        DataProvider.SqlServer.Ops.Context>(_ =>_.UseSqlServer(opsCs));
                    services.AddDbContextPool<PromenadeContext, 
                        DataProvider.SqlServer.Promenade.Context>(_ => _.UseSqlServer(promCs));
                    break;
                case "SQLite":
                    _logger.LogInformation("Using SQLite data provider");
                    services.AddDbContextPool<OpsContext, 
                        DataProvider.SQLite.Ops.Context>(_ => _.UseSqlite(opsCs));
                    services.AddDbContextPool<PromenadeContext, 
                        DataProvider.SQLite.Promenade.Context>(_ => _.UseSqlite(promCs));
                    break;
                default:
                    _logger.LogCritical($"No {Utility.Keys.Configuration.OpsDatabaseProvider} configured in settings. Exiting.");
                    throw new Exception($"No {Utility.Keys.Configuration.OpsDatabaseProvider} configured.");
            }

            var sessionTimeout = TimeSpan.FromHours(2 * 60);
            if(int.TryParse(_config[Utility.Keys.Configuration.OpsSessionTimeoutMinutes], 
                out int configuredTimeout))
            {
                _logger.LogInformation($"Session timeout configured for {configuredTimeout} minutes");
                sessionTimeout = TimeSpan.FromMinutes(configuredTimeout);
            }

            services.AddSession(_ =>
            {
                _.IdleTimeout = sessionTimeout;
                _.Cookie.HttpOnly = true;
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            // filters
            services.AddScoped<Controllers.Filter.AuthenticationFilter>();
            services.AddScoped<Controllers.Filter.UserFilter>();
            services.AddScoped<Controllers.Filter.SectionFilter>();

            // repositories
            services.AddScoped<Service.Interfaces.Ops.IFileRepository, Data.Ops.FileRepository>();
            services.AddScoped<Service.Interfaces.Ops.ILinkRepository, Data.Ops.LinkRepository>();
            services.AddScoped<Service.Interfaces.Ops.ICategoryRepository, Data.Ops.CategoryRepository>();
            services.AddScoped<Service.Interfaces.Ops.IPageRepository, Data.Ops.PageRepository>();
            services.AddScoped<Service.Interfaces.Ops.IPostRepository, Data.Ops.PostRepository>();
            services.AddScoped<Service.Interfaces.Ops.ISectionRepository, 
                Data.Ops.SectionRepository>();
            services.AddScoped<Service.Interfaces.Ops.ISiteSettingRepository, 
                Data.Ops.SiteSettingRepository>();
            services.AddScoped<Service.Interfaces.Ops.IUserRepository, Data.Ops.UserRepository>();

            // path validator
            services.AddScoped<Controllers.Validator.ISectionPathValidator,
                Controllers.Validator.SectionPathValidator>();

            // services
            services.AddScoped<InitialSetupService>();
            services.AddScoped<InsertSampleDataService>();
            services.AddScoped<RosterService>();
            services.AddScoped<SectionService>();
            services.AddScoped<FileService>();
            services.AddScoped<LinkService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<PostService>();
            services.AddScoped<UserService>();
            services.AddScoped<PageService>();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
