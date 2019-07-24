using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Providers;
using Serilog.Context;

namespace Ocuda.Promenade.Web
{
    public class Startup
    {
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

        public void ConfigureServices(IServiceCollection services)
        {
            // set a default culture of en-US if none is specified
            string culture = _config["Promenade.Culture"] ?? "en-US";
            services.Configure<RequestLocalizationOptions>(_ =>
            {
                _.DefaultRequestCulture
                    = new Microsoft.AspNetCore.Localization.RequestCulture(culture);
            });

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

            string promCs = _config.GetConnectionString("Promenade")
                ?? throw new Exception("ConnectionString:Promenade not configured.");
            switch (_config["Promenade.DatabaseProvider"])
            {
                case "SqlServer":
                    services.AddDbContextPool<PromenadeContext, DataProvider.SqlServer.Promenade.Context>(_ =>
                        _.UseSqlServer(promCs)
                        .ConfigureWarnings(w => w
                            .Throw(throwEvents.ToArray())
                            .Log(logEvents.ToArray())
                            .Ignore(ignoreEvents.ToArray())));
                    break;
                default:
                    throw new System.Exception("No Ops.DatabaseProvider configured.");
            }

            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);

            // service facades
            services.AddScoped<Data.ServiceFacade.Repository<PromenadeContext>>();

            // utilities
            services.AddScoped<IDateTimeProvider, CurrentDateTimeProvider>();

            // repositories
            services.AddScoped<Service.Interfaces.Repositories.ILocationHoursRepository,
                Data.Promenade.LocationHoursRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationHoursOverrideRepository,
                Data.Promenade.LocationHoursOverrideRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IFeatureRepository,
                Data.Promenade.FeatureRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IGroupRepository,
                Data.Promenade.GroupRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationFeatureRepository,
                Data.Promenade.LocationFeatureRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationGroupRepository,
                Data.Promenade.LocationGroupRepository>();
            services.AddScoped<Service.Interfaces.Repositories.ILocationRepository,
                Data.Promenade.LocationRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IPageRepository,
                Data.Promenade.PageRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IUrlRedirectAccessRepository,
                Data.Promenade.UrlRedirectAccessRepository>();
            services.AddScoped<Service.Interfaces.Repositories.IUrlRedirectRepository,
                Data.Promenade.UrlRedirectRepository>();

            // services
            services.AddScoped<LocationService>();
            services.AddScoped<PageService>();
            services.AddScoped<RedirectService>();
        }

        public void Configure(IApplicationBuilder app)
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

            // insert remote address into the log context for each request
            app.Use(async (context, next) =>
            {
                using (LogContext.PushProperty("RemoteAddress", context.Connection.RemoteIpAddress))
                {
                    await next.Invoke();
                }
            });

            // use the culture configured above in services
            app.UseRequestLocalization();

            app.UseStaticFiles();

            app.UseMvc();
        }
    }
}

