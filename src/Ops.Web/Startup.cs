using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Ocuda.Ops.Controllers;
using Ocuda.Ops.Data;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Clients;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Web.JobScheduling;
using Ocuda.Ops.Web.StartupHelper;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Providers;

namespace Ocuda.Ops.Web
{
    public class Startup
    {
        private const string DefaultCulture = "en-US";

        private readonly IConfiguration _config;
        private readonly bool _isDevelopment;

        public Startup(IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _isDevelopment = env.IsDevelopment();
        }

        public void Configure(IApplicationBuilder app,
            Utility.Services.Interfaces.IPathResolverService pathResolver)
        {
            if (pathResolver == null)
            {
                throw new ArgumentNullException(nameof(pathResolver));
            }

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
            var contentUrl = pathResolver.GetPublicContentLink();
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

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(_ =>
            {
                _.MapControllers();
                _.MapHealthChecks("/health");
            });
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability",
                    "CA1506:Avoid excessive class coupling",
            Justification = "Dependency injection")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Vulnerability",
            "S4830:Server certificates should be verified during SSL/TLS connections",
            Justification = "Screenly OSE uses self-signed certificates")]
        public void ConfigureServices(IServiceCollection services)
        {
            // set a default culture of en-US if none is specified
            string culture = _config[Configuration.OpsCulture] ?? DefaultCulture;
            services.Configure<RequestLocalizationOptions>(_ =>
            {
                _.DefaultRequestCulture
                    = new Microsoft.AspNetCore.Localization.RequestCulture(culture);
            });

            services.AddHealthChecks();

            switch (_config[Configuration.OpsDistributedCache])
            {
                case "Redis":
                    string redisConfiguration
                        = _config[Configuration.OpsDistributedCacheRedisConfiguration]
                        ?? throw new OcudaException("Configuration.OpsDistributedCache has Redis selected but Configuration.OpsDistributedCacheRedisConfiguration is not set.");
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
                    _config[Configuration.OcudaRuntimeRedisCacheConfiguration]
                        = redisConfiguration;
                    _config[Configuration.OcudaRuntimeRedisCacheInstance] = instanceName;
                    services.AddStackExchangeRedisCache(_ =>
                    {
                        _.Configuration = redisConfiguration;
                        _.InstanceName = instanceName;
                    });
                    break;

                default:
                    services.AddDistributedMemoryCache();
                    break;
            }

            string opsCs = _config.GetConnectionString("Ops")
                ?? throw new OcudaException("ConnectionString:Ops not configured.");
            string promCs = _config.GetConnectionString("Promenade")
                ?? throw new OcudaException("ConnectionString:Promenade not configured.");

            var provider = _config[Configuration.OpsDatabaseProvider];
            switch (provider)
            {
                case "SqlServer":
                    services.AddDbContextPool<OpsContext,
                        DataProvider.SqlServer.Ops.Context>(_ => _.UseSqlServer(opsCs));
                    services.AddDbContextPool<PromenadeContext,
                        DataProvider.SqlServer.Promenade.Context>(_ => _.UseSqlServer(promCs));
                    services.AddHealthChecks()
                        .AddDbContextCheck<OpsContext>()
                        .AddDbContextCheck<PromenadeContext>();
                    break;

                default:
                    throw new OcudaException("No Configuration.OpsDatabaseProvider configured.");
            }

            // stoer the data protection key in the context
            services.AddDataProtection().PersistKeysToDbContext<OpsContext>();

            var sessionTimeout = TimeSpan.FromHours(2 * 60);
            if (int.TryParse(_config[Configuration.OpsSessionTimeoutMinutes],
                out int configuredTimeout))
            {
                sessionTimeout = TimeSpan.FromMinutes(configuredTimeout);
            }

            services.AddSession(_ =>
            {
                _.IdleTimeout = sessionTimeout;
                _.Cookie.HttpOnly = true;
            });

            _config[Configuration.OcudaRuntimeSessionTimeout] = sessionTimeout.ToString();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(_ =>
                {
                    _.AccessDeniedPath = "/Unauthorized";
                    _.LoginPath = "/Authenticate";
                });

            services.AddAuthorization(_ =>
            {
                _.AddPolicy(nameof(ClaimType.SiteManager),
                    policy => policy.RequireClaim(nameof(ClaimType.SiteManager)));
            });

            if (_isDevelopment)
            {
                services.AddControllersWithViews(_ =>
                        _.ModelBinderProviders.RemoveType<DateTimeModelBinderProvider>())
                    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                    .AddDataAnnotationsLocalization(_ =>
                    {
                        _.DataAnnotationLocalizerProvider = (__, factory)
                            => factory.Create(typeof(i18n.Resources.Shared));
                    })
                    .AddSessionStateTempDataProvider()
                    .AddRazorRuntimeCompilation();
            }
            else
            {
                services.AddControllersWithViews(_ =>
                        _.ModelBinderProviders.RemoveType<DateTimeModelBinderProvider>())
                    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                    .AddDataAnnotationsLocalization(_ =>
                    {
                        _.DataAnnotationLocalizerProvider = (__, factory)
                            => factory.Create(typeof(i18n.Resources.Shared));
                    })
                    .AddSessionStateTempDataProvider();
            }

            services.AddHttpClient<Utility.Abstract.IGoogleClient, Utility.Clients.GoogleClient>();
            services.AddHttpClient<Service.Abstract.IScreenlyClient, ScreenlyClient>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback
                            = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                });

            services.AddScoped<IDateTimeProvider, CurrentDateTimeProvider>();

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
            services.AddScoped<Utility.Email.Sender>();

            // repositories
            services.AddScoped<Service.Interfaces.Ops.Repositories.IClaimGroupRepository,
                Data.Ops.ClaimGroupRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ICoverIssueDetailRepository,
                Data.Ops.CoverIssueDetailRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ICoverIssueHeaderRepository,
                Data.Ops.CoverIssueHeaderRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IDigitalDisplayAssetRepository,
                Data.Ops.DigitalDisplayAssetRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IDigitalDisplayDisplaySetRepository,
                Data.Ops.DigitalDisplayDisplaySetRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IDigitalDisplayAssetSetRepository,
                Data.Ops.DigitalDisplayAssetSetRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IDigitalDisplayItemRepository,
                Data.Ops.DigitalDisplayItemRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IDigitalDisplayRepository,
                Data.Ops.DigitalDisplayRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IDigitalDisplaySetRepository,
                Data.Ops.DigitalDisplaySetRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IEmailRecordRepository,
                Data.Ops.EmailRecordRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IEmailSetupTextRepository,
                Data.Ops.EmailSetupTextRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IEmailTemplateTextRepository,
                Data.Ops.EmailTemplateTextRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IExternalResourceRepository,
                Data.Ops.ExternalResourceRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IFileLibraryRepository,
                Data.Ops.FileLibraryRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IFileRepository,
                Data.Ops.FileRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IFileTypeRepository,
                Data.Ops.FileTypeRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ILinkLibraryRepository,
                Data.Ops.LinkLibraryRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ILinkRepository,
                Data.Ops.LinkRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IPermissionGroupApplicationRepository,
                Data.Ops.PermissionGroupApplicationRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IPermissionGroupPodcastItemRepository,
                Data.Ops.PermissionGroupPodcastItemRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IPermissionGroupPageContentRepository,
                Data.Ops.PermissionGroupPageContentRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IPermissionGroupReplaceFilesRepository,
                Data.Ops.PermissionGroupReplaceFilesRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IPermissionGroupRepository,
                Data.Ops.PermissionGroupRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IPermissionGroupSectionManagerRepository,
                Data.Ops.PermissionGroupSectionManagerRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IPostRepository,
                Data.Ops.PostRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ICategoryRepository,
                Data.Ops.CategoryRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IRosterDetailRepository,
                Data.Ops.RosterDetailRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IRosterHeaderRepository,
                Data.Ops.RosterHeaderRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IScheduleClaimRepository,
                Data.Ops.ScheduleClaimRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IScheduleLogCallDispositionRepository,
                Data.Ops.ScheduleLogCallDispositionRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IScheduleLogRepository,
                Data.Ops.ScheduleLogRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ISectionRepository,
                Data.Ops.SectionRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.ISiteSettingRepository,
                Data.Ops.SiteSettingRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IUserMetadataTypeRepository,
                Data.Ops.UserMetadataTypeRepository>();
            services.AddScoped<Service.Interfaces.Ops.Repositories.IUserRepository,
                Data.Ops.UserRepository>();

            services.AddScoped<Service.Interfaces.Promenade.Repositories.ICarouselButtonLabelRepository,
                Data.Promenade.CarouselButtonLabelRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ICarouselButtonRepository,
                Data.Promenade.CarouselButtonRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ICarouselItemRepository,
                Data.Promenade.CarouselItemRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ICarouselItemTextRepository,
                Data.Promenade.CarouselItemTextRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ICarouselRepository,
                Data.Promenade.CarouselRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ICarouselTemplateRepository,
                Data.Promenade.CarouselTemplateRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ICarouselTextRepository,
                Data.Promenade.CarouselTextRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ICategoryRepository,
                Data.Promenade.CategoryRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IEmediaCategoryRepository,
                Data.Promenade.EmediaCategoryRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IEmediaGroupRepository,
                Data.Promenade.EmediaGroupRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IEmediaRepository,
                Data.Promenade.EmediaRepository>();
            services.AddScoped<
                Service.Interfaces.Promenade.Repositories.IExternalResourcePromRepository,
                Data.Promenade.ExternalResourcePromRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IFeatureRepository,
                Data.Promenade.FeatureRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IGroupRepository,
                Data.Promenade.GroupRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IImageFeatureItemRepository,
                Data.Promenade.ImageFeatureItemRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IImageFeatureItemTextRepository,
                Data.Promenade.ImageFeatureItemTextRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IImageFeatureRepository,
                Data.Promenade.ImageFeatureRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IImageFeatureTemplateRepository,
                Data.Promenade.ImageFeatureTemplateRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ILanguageRepository,
                Data.Promenade.LanguageRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ILocationRepository,
               Data.Promenade.LocationRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ILocationFeatureRepository,
                Data.Promenade.LocationFeatureRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ILocationGroupRepository,
                Data.Promenade.LocationGroupRepository>();
            services.AddScoped<
                Service.Interfaces.Promenade.Repositories.ILocationHoursOverrideRepository,
                Data.Promenade.LocationHoursOverrideRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ILocationHoursRepository,
                Data.Promenade.LocationHoursRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IPageRepository,
                Data.Promenade.PageRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IPageHeaderRepository,
                Data.Promenade.PageHeaderRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IPageItemRepository,
                Data.Promenade.PageItemRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IPageLayoutRepository,
                Data.Promenade.PageLayoutRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IPageLayoutTextRepository,
                Data.Promenade.PageLayoutTextRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IPodcastRepository,
                Data.Promenade.PodcastRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IPodcastItemsRepository,
                Data.Promenade.PodcastItemsRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IScheduleRequestLimitRepository,
                Data.Promenade.ScheduleRequestLimitRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IScheduleRequestRepository,
                Data.Promenade.ScheduleRequestRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.IScheduleRequestSubjectRepository,
                Data.Promenade.ScheduleRequestSubjectRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ISegmentRepository,
                Data.Promenade.SegmentRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ISegmentTextRepository,
                Data.Promenade.SegmentTextRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ISiteSettingPromRepository,
                Data.Promenade.SiteSettingPromRepository>();
            services.AddScoped<Service.Interfaces.Promenade.Repositories.ISocialCardRepository,
                Data.Promenade.SocialCardRepository>();

            // services
            services.AddScoped<Service.Interfaces.Ops.Services.IAuthorizationService,
                AuthorizationService>();
            services.AddScoped<ICarouselService, CarouselService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICoverIssueService, CoverIssueService>();
            services.AddScoped<IDigitalDisplayService, DigitalDisplayService>();
            services.AddScoped<IDigitalDisplaySyncService, DigitalDisplaySyncService>();
            services.AddScoped<IEmediaService, EmediaService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IExternalResourcePromService, ExternalResourcePromService>();
            services.AddScoped<IExternalResourceService, ExternalResourceService>();
            services.AddScoped<IFeatureService, FeatureService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IFileTypeService, FileTypeService>();
            services.AddScoped<IGroupService, GroupService>();
            services.AddScoped<IImageFeatureService, ImageFeatureService>();
            services.AddScoped<IInitialSetupService, InitialSetupService>();
            services.AddScoped<IInsertSampleDataService, InsertSampleDataService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<ILdapService, LdapService>();
            services.AddScoped<ILocationService, LocationService>();
            services.AddScoped<ILocationHoursService, LocationHoursService>();
            services.AddScoped<ILocationGroupService, LocationGroupService>();
            services.AddScoped<ILocationFeatureService, LocationFeatureService>();
            services.AddScoped<ILinkService, LinkService>();
            services.AddScoped<Utility.Services.Interfaces.IOcudaCache,
                Utility.Services.OcudaCache>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IPublicFilesService, PublicFilesService>();
            services.AddScoped<Utility.Services.Interfaces.IPathResolverService,
                Utility.Services.PathResolverService>();
            services.AddScoped<IPodcastService, PodcastService>();
            services.AddScoped<IPostService, PostService>();
            services.AddScoped<IPermissionGroupService, PermissionGroupService>();
            services.AddScoped<IRosterService, RosterService>();
            services.AddScoped<IScheduleNotificationService, ScheduleNotificationService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<IScheduleRequestLimitService,
                ScheduleRequestLimitService>();
            services.AddScoped<Service.Interfaces.Ops.Services.IScheduleRequestService,
                ScheduleRequestService>();
            services.AddScoped<IScreenlyService, ScreenlyService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<ISegmentService, SegmentService>();
            services.AddScoped<ISiteSettingPromService, SiteSettingPromService>();
            services.AddScoped<ISiteSettingService, SiteSettingService>();
            services.AddScoped<ISocialCardService, SocialCardService>();
            services.AddScoped<Service.Abstract.IUserContextProvider, UserContextProvider>();
            services.AddScoped<IUserMetadataTypeService, UserMetadataTypeService>();
            services.AddScoped<IUserService, UserService>();

            // background process
            services.AddScoped<JobScopedProcessingService>();
            services.AddHostedService<JobBackgroundService>();
        }
    }
}