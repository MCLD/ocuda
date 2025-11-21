using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Ocuda.i18n;
using Ocuda.i18n.RouteConstraint;
using Ocuda.Promenade.Controllers;
using Ocuda.Promenade.Data;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Clients;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Providers;
using Serilog;
using Serilog.Context;

namespace Ocuda.Promenade.Web
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly bool _isDevelopment;
        private RouteDataRequestCultureProvider _routeDataCultureProvider;

        public Startup(IConfiguration configuration,
            IWebHostEnvironment env)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(env);

            _config = configuration;
            _isDevelopment = env.IsDevelopment();
        }

        public void Configure(IApplicationBuilder app,
            Utility.Services.Interfaces.IPathResolverService pathResolver)
        {
            ArgumentNullException.ThrowIfNull(app);
            ArgumentNullException.ThrowIfNull(pathResolver);

            if (!_isDevelopment)
            {
                app.UseResponseCompression();
            }

            app.UseWebOptimizer();

            if (!string.IsNullOrEmpty(_config[Configuration.OcudaProxyAddress]))
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.All,
                    RequireHeaderSymmetry = false,
                    ForwardLimit = null,
                    KnownProxies = {
                        System.Net.IPAddress.Parse(_config[Configuration.OcudaProxyAddress])
                    }
                });
            }

            // configure error page handling and development IDE linking
            if (_isDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            var requestLocalizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(Culture.DefaultCulture),
                SupportedCultures = Culture.SupportedCultures,
                SupportedUICultures = Culture.SupportedCultures,
            };

            requestLocalizationOptions.RequestCultureProviders.Add(_routeDataCultureProvider);

            requestLocalizationOptions
                .RequestCultureProviders
                .Remove(requestLocalizationOptions
                    .RequestCultureProviders
                    .Single(_ => _.GetType() == typeof(QueryStringRequestCultureProvider)));

            app.UseRequestLocalization(requestLocalizationOptions);

            // insert remote address into the log context for each request
            app.Use(async (context, next) =>
            {
                using (LogContext.PushProperty(Utility.Logging.Enrichment.RemoteAddress,
                    context.Connection.RemoteIpAddress))
                using (LogContext.PushProperty(HeaderNames.UserAgent,
                    context.Request.Headers[HeaderNames.UserAgent].ToString()))
                using (LogContext.PushProperty(HeaderNames.Referer,
                    context.Request.Headers[HeaderNames.Referer].ToString()))
                {
                    await next.Invoke();
                }
            });

            app.UseStaticFiles();

            // configure shared content directory
            var contentFilePath = pathResolver.GetPublicContentFilePath();
            var contentUrl = pathResolver.GetPublicContentLink();
            if (!contentUrl.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                contentUrl = $"/{contentUrl}";
            }

            // https://github.com/aspnet/AspNetCore/issues/2442
            var extensionContentTypeProvider = new FileExtensionContentTypeProvider();
            extensionContentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider
                    = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(contentFilePath),
                RequestPath = new PathString(contentUrl),
                ContentTypeProvider = extensionContentTypeProvider
            });

            if (!string.IsNullOrEmpty(_config[Configuration.PromenadeRequestLogging]))
            {
                app.UseSerilogRequestLogging();
            }

            app.UseSession();

            app.UseHealthChecks("/healthcheck");
            app.UseMvc();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability",
                    "CA1506:Avoid excessive class coupling",
            Justification = "Dependency injection")]
        public void ConfigureServices(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            if (_isDevelopment)
            {
                services.AddApplicationInsightsTelemetry();
            }
            else
            {
                services.AddResponseCompression(_ =>
                {
                    _.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                            new[] {
                                "application/rss+xml",
                                "image/svg+xml"
                            });
                });
            }

            services.AddLocalization();

            services.Configure<RequestLocalizationOptions>(_ =>
            {
                _.DefaultRequestCulture = new RequestCulture(Culture.DefaultCulture);
                _.SupportedCultures = Culture.SupportedCultures;
                _.SupportedUICultures = Culture.SupportedCultures;
                _routeDataCultureProvider = new RouteDataRequestCultureProvider
                {
                    Options = _
                };
                _.RequestCultureProviders.Insert(0, _routeDataCultureProvider);
                _.RequestCultureProviders
                    .Remove(_.RequestCultureProviders
                        .Single(__ => __.GetType() == typeof(QueryStringRequestCultureProvider)));
            });

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddHealthChecks();

            // configure distributed cache
            if (_config[Configuration.PromenadeDistributedCache]?.ToUpperInvariant() == "REDIS")
            {
                string redisConfiguration
                    = _config[Configuration.PromenadeDistributedCacheRedisConfiguration]
                    ?? throw new OcudaException("Configuration.PromenadeDistributedCache has Redis selected but Configuration.PromenadeDistributedCacheRedisConfiguration is not set.");
                string instanceName = CacheInstance.OcudaPromenade;
                if (!instanceName.EndsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    instanceName += ".";
                }
                string cacheDiscriminator
                    = _config[Configuration.PromenadeDistributedCacheInstanceDiscriminator]
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
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            var sessionTimeout = TimeSpan.FromMinutes(20);
            if (int.TryParse(_config[Configuration.PromenadeSessionTimeoutMinutes],
                out int configuredTimeout))
            {
                sessionTimeout = TimeSpan.FromMinutes(configuredTimeout);
            }

            string cookieName = string.IsNullOrEmpty(_config[Configuration.OcudaCookieName])
                ? ".oc"
                : _config[Configuration.OcudaCookieName];

            services.AddSession(_ =>
            {
                _.IdleTimeout = sessionTimeout;
                _.Cookie.HttpOnly = true;
                _.Cookie.IsEssential = true;
                _.Cookie.Name = cookieName;
            });

            // database configuration
            string promCs = _config.GetConnectionString("Promenade")
                ?? throw new OcudaException("ConnectionString:Promenade not configured.");

            if (_config[Configuration.PromenadeDatabaseProvider]?.ToUpperInvariant()
                    == "SQLSERVER")
            {
                var poolSizeConfig = _config[Configuration.PromenadeDatabasePoolSize];
                if (!string.IsNullOrEmpty(poolSizeConfig)
                    && int.TryParse(poolSizeConfig, out int poolSize))
                {
                    if (_isDevelopment)
                    {
                        services.AddDbContextPool<PromenadeContext,
                            DataProvider.SqlServer.Promenade.Context>(_ => _
                                .UseSqlServer(promCs)
                                .AddInterceptors(new DbLoggingInterceptor()),
                                poolSize);
                        services.AddHealthChecks();
                    }
                    else
                    {
                        services.AddDbContextPool<PromenadeContext,
                            DataProvider.SqlServer.Promenade.Context>(_ => _.UseSqlServer(promCs),
                                poolSize);
                    }
                }
                else
                {
                    if (_isDevelopment)
                    {
                        services.AddDbContextPool<PromenadeContext,
                            DataProvider.SqlServer.Promenade.Context>(_ => _
                                .UseSqlServer(promCs)
                                .AddInterceptors(new DbLoggingInterceptor()));
                        services.AddHealthChecks();
                    }
                    else
                    {
                        services.AddDbContextPool<PromenadeContext,
                            DataProvider.SqlServer.Promenade.Context>(_ => _.UseSqlServer(promCs));
                    }
                }
            }
            else
            {
                throw new OcudaException("No Configuration.PromenadeDatabaseProvider configured.");
            }

            services.AddDataProtection().PersistKeysToDbContext<PromenadeContext>();

            services.Configure<RouteOptions>(_ =>
            {
                _.ConstraintMap.Add("cultureConstraint", typeof(CultureRouteConstraint));
                _.ConstraintMap.Add(LocationSlugRouteConstraint.Name,
                    typeof(LocationSlugRouteConstraint));
            });

            if (_isDevelopment)
            {
                services.AddControllersWithViews(_ =>
                    {
                        _.ModelBinderProviders.RemoveType<DateTimeModelBinderProvider>();
                        _.EnableEndpointRouting = false;
                    })
                    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                    .AddDataAnnotationsLocalization(_ =>
                    {
                        _.DataAnnotationLocalizerProvider = (__, factory)
                            => factory.Create(typeof(i18n.Resources.Shared));
                    })
                    .AddRazorRuntimeCompilation()
                    .AddSessionStateTempDataProvider();
            }
            else
            {
                services.AddControllersWithViews(_ =>
                    {
                        _.ModelBinderProviders.RemoveType<DateTimeModelBinderProvider>();
                        _.EnableEndpointRouting = false;
                    })
                    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                    .AddDataAnnotationsLocalization(_ =>
                    {
                        _.DataAnnotationLocalizerProvider = (__, factory)
                            => factory.Create(typeof(i18n.Resources.Shared));
                    })
                    .AddSessionStateTempDataProvider();
            }

            services.AddWebOptimizer(_ =>
            {
                _.AddFiles("text/javascript", "/js/*");
                _.AddFiles("text/css", "/css/*");

                _.AddJavaScriptBundle("/js/scripts.min.js",
                    "js/jquery.js",
                    "js/jquery.validate.js",
                    "js/jquery.validate.unobtrusive.js",
                    "js/popper.js",
                    "js/slick.js",
                    "Scripts/script.js"
                    ).UseContentRoot();

                // minifying Bootstrap seems to upset this tool, bring it in pre-minified
                _.AddJavaScriptBundle("/js/bootstrap.min.js",
                    new WebOptimizer.Processors.JsSettings
                    {
                        CodeSettings = new NUglify.JavaScript.CodeSettings { MinifyCode = false }
                    },
                    "js/bootstrap.min.js").UseContentRoot();

                // minifying Bootstrap seems to upset this tool, bring it in pre-minified
                _.AddCssBundle("/css/bootstrap.min.css",
                    new NUglify.Css.CssSettings { MinifyExpressions = false },
                    "css/bootstrap.min.css").UseContentRoot();

                _.AddCssBundle("/css/styles.min.css",
                    "css/all.css",
                    "css/slick.css",
                    "css/slick-theme.css",
                    "Styles/style.css"
                    ).UseContentRoot();
            });

            services.Configure<RouteOptions>(_ =>
            {
                _.AppendTrailingSlash = true;
                _.LowercaseUrls = true;
            });

            // service facades
            services.AddScoped(typeof(Controllers.ServiceFacades.Controller<>));
            services.AddScoped(typeof(Controllers.ServiceFacades.PageController));
            services.AddScoped(typeof(Data.ServiceFacade.Repository<>));

            // utilities
            services.AddScoped<IDateTimeProvider, CurrentDateTimeProvider>();
            services.AddScoped<Utility.Services.Interfaces.IOcudaCache,
                Utility.Services.OcudaCache>();

            services.AddHttpClient<IGoogleClient, GoogleClient>();

            // filters
            services.AddScoped<i18n.Filter.LocalizationFilterAttribute>();
            services.AddScoped<Controllers.Filters.LayoutFilter>();

            // repositories
            services.AddScoped<Service.Interfaces.Repositories.ICardDetailRepository,
                Data.Promenade.CardDetailRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ICardRepository,
                Data.Promenade.CardRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ICarouselButtonLabelTextRepository,
                Data.Promenade.CarouselButtonLabelTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ICarouselItemRepository,
                Data.Promenade.CarouselItemRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ICarouselItemTextRepository,
                Data.Promenade.CarouselItemTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ICarouselRepository,
                Data.Promenade.CarouselRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ICarouselTemplateRepository,
                Data.Promenade.CarouselTemplateRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ICarouselTextRepository,
                Data.Promenade.CarouselTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ICategoryRepository,
                Data.Promenade.CategoryRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ICategoryTextRepository,
                Data.Promenade.CategoryTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IEmediaRepository,
                Data.Promenade.EmediaRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IEmediaCategoryRepository,
                Data.Promenade.EmediaCategoryRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IEmediaGroupRepository,
                Data.Promenade.EmediaGroupRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IEmediaTextRepository,
                Data.Promenade.EmediaTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IExternalResourceRepository,
                Data.Promenade.ExternalResourceRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IEmployeeCardRequestRepository,
                Data.Promenade.EmployeeCardRequestRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IFeatureRepository,
                Data.Promenade.FeatureRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IVolunteerFormRepository,
                Data.Promenade.VolunteerFormRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IVolunteerFormSubmissionRepository,
                Data.Promenade.VolunteerFormSubmissionRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IGroupRepository,
                Data.Promenade.GroupRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILanguageRepository,
                Data.Promenade.LanguageRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationRepository,
                Data.Promenade.LocationRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationFeatureRepository,
                Data.Promenade.LocationFeatureRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationFormRepository,
                Data.Promenade.LocationFormRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationGroupRepository,
                Data.Promenade.LocationGroupRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationHoursRepository,
                Data.Promenade.LocationHoursRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationHoursOverrideRepository,
                Data.Promenade.LocationHoursOverrideRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationInteriorImageRepository,
                Data.Promenade.LocationInteriorImageRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IImageAltTextRepository,
                Data.Promenade.ImageAltTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.INavBannerRepository,
                Data.Promenade.NavBannerRepository>();
            services.AddScoped<Service.Interfaces.Repositories.INavBannerImageRepository,
                Data.Promenade.NavBannerImageRepository>();
            services.AddScoped<Service.Interfaces.Repositories.INavBannerLinkRepository,
                Data.Promenade.NavBannerLinkRepository>();
            services.AddScoped<Service.Interfaces.Repositories.INavBannerLinkTextRepository,
                Data.Promenade.NavBannerLinkTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationInteriorImageRepository,
                Data.Promenade.LocationInteriorImageRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IImageAltTextRepository,
                Data.Promenade.ImageAltTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.INavigationRepository,
                Data.Promenade.NavigationRepository>();
            services.AddScoped<Service.Interfaces.Repositories.INavigationTextRepository,
                Data.Promenade.NavigationTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IPageHeaderRepository,
                Data.Promenade.PageHeaderRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IImageFeatureItemRepository,
                Data.Promenade.ImageFeatureItemRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IImageFeatureItemTextRepository,
                Data.Promenade.ImageFeatureItemTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IImageFeatureRepository,
                Data.Promenade.ImageFeatureRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IImageFeatureTemplateRepository,
                Data.Promenade.ImageFeatureTemplateRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IPageLayoutRepository,
                Data.Promenade.PageLayoutRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IPageLayoutTextRepository,
                Data.Promenade.PageLayoutTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IPageRepository,
                Data.Promenade.PageRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IPodcastItemRepository,
                Data.Promenade.PodcastItemRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IPodcastRepository,
                Data.Promenade.PodcastRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IProductInventoryRepository,
                Data.Promenade.ProductInventoryRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IProductRepository,
                Data.Promenade.ProductRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IScheduleRequestLimitRepository,
                Data.Promenade.ScheduleRequestLimitRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IScheduleRequestRepository,
                Data.Promenade.ScheduleRequestRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IScheduleRequestSubjectRepository,
                Data.Promenade.ScheduleRequestSubjectRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IScheduleRequestSubjectTextRepository,
                Data.Promenade.ScheduleRequestSubjectTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IScheduleRequestTelephoneRepository,
                Data.Promenade.ScheduleRequestTelephoneRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ISegmentRepository,
                Data.Promenade.SegmentRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ISegmentTextRepository,
                Data.Promenade.SegmentTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ISegmentWrapRepository,
                Data.Promenade.SegmentWrapRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ISiteAlertRepository,
                Data.Promenade.SiteAlertRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ISiteSettingRepository,
                Data.Promenade.SiteSettingRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ISocialCardRepository,
                Data.Promenade.SocialCardRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IUrlRedirectAccessRepository,
                Data.Promenade.UrlRedirectAccessRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IUrlRedirectRepository,
                Data.Promenade.UrlRedirectRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IImageFeatureItemRepository,
                Data.Promenade.ImageFeatureItemRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IImageFeatureItemTextRepository,
                Data.Promenade.ImageFeatureItemTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IImageFeatureRepository,
                Data.Promenade.ImageFeatureRepository>();

            // services
            services.AddScoped<Utility.Services.Interfaces.IPathResolverService,
                Utility.Services.PathResolverService>();

            // promenade services
            services.AddScoped<CarouselService>();
            services.AddScoped<CategoryService>();
            services.AddScoped<DeckService>();
            services.AddScoped<EmediaService>();
            services.AddScoped<EmployeeCardService>();
            services.AddScoped<ExternalResourceService>();
            services.AddScoped<ImageFeatureService>();
            services.AddScoped<LanguageService>();
            services.AddScoped<LocationService>();
            services.AddScoped<NavBannerService>();
            services.AddScoped<NavigationService>();
            services.AddScoped<ImageFeatureService>();
            services.AddScoped<PageService>();
            services.AddScoped<RedirectService>();
            services.AddScoped<PodcastService>();
            services.AddScoped<ProductService>();
            services.AddScoped<ScheduleService>();
            services.AddScoped<SegmentService>();
            services.AddScoped<SiteAlertService>();
            services.AddScoped<SiteSettingService>();
            services.AddScoped<SocialCardService>();
            services.AddScoped<VolunteerFormService>();
        }
    }
}