using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Providers;
using Serilog.Context;

namespace Ocuda.Promenade.Web
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            string promCs = _config.GetConnectionString("Promenade")
                ?? throw new Exception("ConnectionString:Promenade not configured.");
            switch (_config["Promenade.DatabaseProvider"])
            {
                case "SqlServer":
                    services.AddDbContextPool<PromenadeContext, DataProvider.SqlServer.Promenade.Context>(_ =>
                        _.UseSqlServer(promCs));
                    break;
                default:
                    throw new System.Exception("No Ops.DatabaseProvider configured.");
            }

            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);

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

            // services
            services.AddScoped<LocationService>();
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

