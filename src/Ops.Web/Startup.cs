using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers;
using Ocuda.Ops.Controllers.Authorization;
using Ocuda.Ops.Data;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Web.StartupHelper;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Web
{
    public class Startup
    {
        private const string DefaultCulture = "en-US";

        private readonly IConfiguration _config;
        private readonly bool _isDevelopment;
        private readonly ILogger _logger;

        public Startup(IConfiguration configuration,
            IHostingEnvironment env,
            ILogger<Startup> logger)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _isDevelopment = env.IsDevelopment();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // set a default culture of en-US if none is specified
            string culture = _config[Configuration.OpsCulture] ?? DefaultCulture;
            _logger.LogInformation("Configuring for culture: {0}", culture);
            services.Configure<RequestLocalizationOptions>(_ =>
            {
                _.DefaultRequestCulture
                    = new Microsoft.AspNetCore.Localization.RequestCulture(culture);
            });

            switch (_config[Configuration.OpsDistributedCache])
            {
                case "Redis":
                    string redisConfiguration
                        = _config[Configuration.OpsDistributedCacheRedisConfiguration]
                        ?? throw new OcudaException($"{Configuration.OpsDistributedCache} has Redis selected but {Configuration.OpsDistributedCacheRedisConfiguration} is not set.");
                    string instanceName = CacheInstance.OcudaOps;
                    if (!instanceName.EndsWith(".", StringComparison.OrdinalIgnoreCase))
                    {
                        instanceName += ".";
                    }
                    string cacheDiscriminator
                        = _config[Configuration.OpsDistributedCacheInstanceDiscriminator]
                        ?? string.Empty;
                    if (!string.IsNullOrEmpty(cacheDiscriminator))
                    {
                        instanceName = $"{instanceName}{cacheDiscriminator}.";
                    }
                    _logger.LogInformation("Using Redis distributed cache {0} instance {1}",
                        redisConfiguration,
                        instanceName);
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

            // configure ef errors to throw, log, or ignore as appropriate for the environment
            // see https://docs.microsoft.com/en-us/ef/core/querying/related-data#ignored-includes
            var throwEvents = new List<EventId>();
            var logEvents = new List<EventId>();
            var ignoreEvents = new List<EventId>();

            if (_isDevelopment)
            {
                if (string.IsNullOrEmpty(_config[Configuration.ThrowQueryWarningsInDev]))
                {
                    logEvents.Add(RelationalEventId.QueryClientEvaluationWarning);
                    logEvents.Add(CoreEventId.FirstWithoutOrderByAndFilterWarning);
                }
                else
                {
                    throwEvents.Add(RelationalEventId.QueryClientEvaluationWarning);
                    throwEvents.Add(CoreEventId.FirstWithoutOrderByAndFilterWarning);
                }

                throwEvents.Add(CoreEventId.IncludeIgnoredWarning);
            }
            else
            {
                logEvents.Add(RelationalEventId.QueryClientEvaluationWarning);
                logEvents.Add(CoreEventId.IncludeIgnoredWarning);
            }

            string opsCs = _config.GetConnectionString("Ops")
                ?? throw new OcudaException("ConnectionString:Ops not configured.");
            string promCs = _config.GetConnectionString("Promenade")
                ?? throw new OcudaException("ConnectionString:Promenade not configured.");

            var provider = _config[Configuration.OpsDatabaseProvider];
            switch (provider)
            {
                case "SqlServer":
                    _logger.LogInformation("Using {0} data provider", provider);
                    services.AddDbContextPool<OpsContext,
                        DataProvider.SqlServer.Ops.Context>(_ => _.UseSqlServer(opsCs)
                        .ConfigureWarnings(w => w
                            .Throw(throwEvents.ToArray())
                            .Log(logEvents.ToArray())
                            .Ignore(ignoreEvents.ToArray())));
                    services.AddDbContextPool<PromenadeContext,
                        DataProvider.SqlServer.Promenade.Context>(_ => _.UseSqlServer(promCs)
                        .ConfigureWarnings(w => w
                            .Throw(throwEvents.ToArray())
                            .Log(logEvents.ToArray())
                            .Ignore(ignoreEvents.ToArray())));
                    break;
                default:
                    _logger.LogCritical("No {0} configured in settings. Exiting.",
                        Configuration.OpsDatabaseProvider);
                    throw new OcudaException($"No {Configuration.OpsDatabaseProvider} configured.");
            }

            // stoer the data protection key in the context
            services.AddDataProtection().PersistKeysToDbContext<OpsContext>();

            var sessionTimeout = TimeSpan.FromHours(2 * 60);
            if (int.TryParse(_config[Configuration.OpsSessionTimeoutMinutes],
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
                    _.AccessDeniedPath = "/Unauthorized";
                    _.LoginPath = "/Authenticate";
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
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2)
                .AddSessionStateTempDataProvider();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // service facades
            services.AddScoped(typeof(Controllers.ServiceFacades.Controller<>));
            services.AddScoped(typeof(Data.ServiceFacade.Repository<>));

            // filters
            services.AddScoped<Controllers.Filters.AuthenticationFilterAttribute>();
            services.AddScoped<Controllers.Filters.ExternalResourceFilterAttribute>();
            services.AddScoped<Controllers.Filters.NavigationFilterAttribute>();
            services.AddScoped<Controllers.Filters.UserFilterAttribute>();

            // helpers
            services.AddScoped<Utility.Helpers.WebHelper>();

            // repositories
            services.AddScoped<Service.Interfaces.Ops.Repositories.IClaimGroupRepository,
                Data.Ops.ClaimGroupRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ICoverIssueDetailRepository,
                Data.Ops.CoverIssueDetailRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ICoverIssueHeaderRepository,
                Data.Ops.CoverIssueHeaderRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IExternalResourceRepository,
                Data.Ops.ExternalResourceRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IFeatureRepository,
                Data.Ops.FeatureRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IFileLibraryRepository,
                Data.Ops.FileLibraryRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IFileRepository,
                Data.Ops.FileRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IFileTypeRepository,
                Data.Ops.FileTypeRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IGroupRepository,
                Data.Ops.GroupRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ILocationRepository,
                Data.Ops.LocationRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ILocationFeatureRepository,
                Data.Ops.LocationFeatureRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ILocationGroupRepository,
                Data.Ops.LocationGroupRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ILocationHoursRepository,
                Data.Ops.LocationHoursRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ILinkLibraryRepository,
                Data.Ops.LinkLibraryRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ILinkRepository,
                Data.Ops.LinkRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IPostRepository,
                Data.Ops.PostRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ICategoryRepository,
                Data.Ops.CategoryRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IRosterDetailRepository,
                Data.Ops.RosterDetailRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IRosterHeaderRepository,
                Data.Ops.RosterHeaderRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ISectionRepository,
                Data.Ops.SectionRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ISectionManagerGroupRepository,
                Data.Ops.SectionManagerGroupRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ISiteSettingRepository,
                Data.Ops.SiteSettingRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IUserMetadataTypeRepository,
                Data.Ops.UserMetadataTypeRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IUserRepository,
                Data.Ops.UserRepository>();

            services.AddScoped<Service.Interfaces.Promenade.Repositories.ICategoryRepository,
                Data.Promenade.CategoryRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IEmediaCategoryRepository,
                Data.Promenade.EmediaCategoryRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IEmediaRepository,
                Data.Promenade.EmediaRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ILanguageRepository,
                Data.Promenade.LanguageRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IPageRepository,
                Data.Promenade.PageRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IPageHeaderRepository,
                Data.Promenade.PageHeaderRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ISocialCardRepository,
                Data.Promenade.SocialCardRepository>();

            // services
            services.AddScoped<Service.Interfaces.Ops.Services.IAuthorizationService,
                AuthorizationService>();
            services.AddScoped<ICoverIssueService, CoverIssueService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IEmediaService, EmediaService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IExternalResourceService, ExternalResourceService>();
            services.AddScoped<IFeatureService, FeatureService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IFileTypeService, FileTypeService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IInitialSetupService, InitialSetupService>();
            services.AddScoped<IInsertSampleDataService, InsertSampleDataService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<ILdapService, LdapService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<ILocationHoursService, LocationHoursService>();
            services.AddScoped<ILocationGroupService, LocationGroupService>();
            services.AddScoped<ILocationFeatureService, LocationFeatureService>();
            services.AddScoped<ILinkService, LinkService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<Utility.Services.Interfaces.IPathResolverService,
                Utility.Services.PathResolverService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IRosterService, RosterService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<ISiteSettingService, SiteSettingService>();
            services.AddScoped<ISocialCardService, SocialCardService>();
            services.AddScoped<Service.Abstract.IUserContextProvider, UserContextProvider>();
            services.AddScoped<IUserMetadataTypeService, UserMetadataTypeService>();
            services.AddScoped<IUserService, UserService>();

            return services.BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app,
            Utility.Services.Interfaces.IPathResolverService pathResolver)
        {
            // configure error page handling and development IDE linking
            if (_isDevelopment)
            {
                app.UseDeveloperExceptionPage();
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider
                    = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                        Path.Combine(Path.GetFullPath("Styles"))),
                    RequestPath = new PathString("/devstyles")
                });
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider
                    = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                        Path.Combine(Path.GetFullPath("Scripts"))),
                    RequestPath = new PathString("/devscripts")
                });
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider
                    = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
                        Path.Combine(Path.GetFullPath("node_modules"))),
                    RequestPath = new PathString("/devmodules")
                });
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
            if (!contentUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                contentUrl = $"/{contentUrl}";
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider
                    = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(contentFilePath),
                RequestPath = new PathString(contentUrl)
            });

            app.UseSession();

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
