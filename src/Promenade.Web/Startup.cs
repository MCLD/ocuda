using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Ocuda.i18n;
using Ocuda.i18n.RouteConstraint;
using Ocuda.Promenade.Data;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Providers;
using Serilog.Context;

namespace Ocuda.Promenade.Web
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly bool _isDevelopment;

        public Startup(IConfiguration configuration,
            IWebHostEnvironment env)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _isDevelopment = env.IsDevelopment();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // build a temporary logger for this method call
            using var logger = Utility.Logging.Configuration.Build(_config).CreateLogger();

            services.AddLocalization();

            services.Configure<RequestLocalizationOptions>(_ =>
            {
                _.DefaultRequestCulture = new RequestCulture(Culture.DefaultCulture);
                _.SupportedCultures = Culture.SupportedCultures;
                _.SupportedUICultures = Culture.SupportedCultures;
                _.RequestCultureProviders.Insert(0,
                    new RouteDataRequestCultureProvider { Options = _ });
                _.RequestCultureProviders
                    .Remove(_.RequestCultureProviders
                        .Single(p => p.GetType() == typeof(QueryStringRequestCultureProvider)));
            });

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            string promCs = _config.GetConnectionString("Promenade")
                ?? throw new OcudaException("ConnectionString:Promenade not configured.");

            var provider = _config[Configuration.PromenadeDatabaseProvider];
            if (provider == "SqlServer")
            {
                logger.Information("Using {0} data provider", provider);
                services.AddDbContextPool<PromenadeContext, DataProvider.SqlServer.Promenade.Context>(_ =>
                    _.UseSqlServer(promCs));
            }
            else
            {
                logger.Fatal("No {0} configured in settings. Exiting.",
                    Configuration.PromenadeDatabaseProvider);
                throw new OcudaException($"No {Configuration.PromenadeDatabaseProvider} configured.");
            }

            services.Configure<RouteOptions>(_ =>
                _.ConstraintMap.Add("cultureConstraint", typeof(CultureRouteConstraint)));

            services.AddControllersWithViews()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(_ =>
                {
                    _.DataAnnotationLocalizerProvider = (__, factory)
                        => factory.Create(typeof(i18n.Resources.Shared));
                });

            services.Configure<RouteOptions>(_ =>
            {
                _.AppendTrailingSlash = true;
                _.LowercaseUrls = true;
            });

            // service facades
            services.AddScoped(typeof(Controllers.ServiceFacades.Controller<>));
            services.AddScoped(typeof(Data.ServiceFacade.Repository<>));

            // utilities
            services.AddScoped<CultureContextProvider>();
            services.AddScoped<IDateTimeProvider, CurrentDateTimeProvider>();

            // filters
            services.AddScoped<i18n.Filter.LocalizationFilterAttribute>();
            services.AddScoped<Controllers.Filters.LayoutFilter>();

            // repositories
            services.AddScoped<Service.Interfaces.Repositories.ICategoryRepository,
                Data.Promenade.CategoryRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IEmediaRepository,
                Data.Promenade.EmediaRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IEmediaCategoryRepository,
                Data.Promenade.EmediaCategoryRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IExternalResourceRepository,
                Data.Promenade.ExternalResourceRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IFeatureRepository,
                Data.Promenade.FeatureRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IGroupRepository,
                Data.Promenade.GroupRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILanguageRepository,
                Data.Promenade.LanguageRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationRepository,
                Data.Promenade.LocationRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationFeatureRepository,
                Data.Promenade.LocationFeatureRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationGroupRepository,
                Data.Promenade.LocationGroupRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationHoursRepository,
                Data.Promenade.LocationHoursRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationHoursOverrideRepository,
                Data.Promenade.LocationHoursOverrideRepository>();
            services.AddScoped<Service.Interfaces.Repositories.INavigationRepository,
                Data.Promenade.NavigationRepository>();
            services.AddScoped<Service.Interfaces.Repositories.INavigationTextRepository,
                Data.Promenade.NavigationTextRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IPageRepository,
                Data.Promenade.PageRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ISegmentRepository,
                Data.Promenade.SegmentRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ISegmentTextRepository,
                Data.Promenade.SegmentTextRepository>();
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

            // services
            services.AddScoped<Utility.Services.Interfaces.IPathResolverService,
                Utility.Services.PathResolverService>();

            // promenade servicews
            services.AddScoped<CategoryService>();
            services.AddScoped<EmediaService>();
            services.AddScoped<ExternalResourceService>();
            services.AddScoped<LanguageService>();
            services.AddScoped<LocationService>();
            services.AddScoped<NavigationService>();
            services.AddScoped<PageService>();
            services.AddScoped<RedirectService>();
            services.AddScoped<SegmentService>();
            services.AddScoped<SiteAlertService>();
            services.AddScoped<SiteSettingService>();
            services.AddScoped<SocialCardService>();
        }

        public void Configure(IApplicationBuilder app,
            Utility.Services.Interfaces.IPathResolverService pathResolver)
        {
            // configure error page handling and development IDE linking
            if (_isDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

            var requestLocalizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(Culture.DefaultCulture),
                SupportedCultures = Culture.SupportedCultures,
                SupportedUICultures = Culture.SupportedCultures
            };
            requestLocalizationOptions.RequestCultureProviders.Insert(0,
                new RouteDataRequestCultureProvider { Options = requestLocalizationOptions });

            requestLocalizationOptions
                .RequestCultureProviders
                .Remove(requestLocalizationOptions
                    .RequestCultureProviders
                    .Single(_ => _.GetType() == typeof(QueryStringRequestCultureProvider)));

            app.UseRequestLocalization(requestLocalizationOptions);

            // insert remote address into the log context for each request
            app.Use(async (context, next) =>
            {
                using (LogContext.PushProperty("RemoteAddress", context.Connection.RemoteIpAddress))
                {
                    await next.Invoke();
                }
            });

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

            app.UseRouting();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
