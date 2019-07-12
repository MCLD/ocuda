using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocuda.Promenade.Data;
using Serilog.Context;

namespace Ocuda.Promenade.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // set a default culture of en-US if none is specified
            string culture = Configuration["Promenade.Culture"] ?? "en-US";
            services.Configure<RequestLocalizationOptions>(_ =>
            {
                _.DefaultRequestCulture
                    = new Microsoft.AspNetCore.Localization.RequestCulture(culture);
            });

            switch (Configuration["Promenade.DatabaseProvider"])
            {
                case "SqlServer":
                    services.AddDbContextPool<PromenadeContext, DataProvider.SqlServer.Promenade.Context>(_ =>
                        _.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;"));
                    break;
                default:
                    throw new System.Exception("No Ops.DatabaseProvider configured.");
            }

            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);
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

