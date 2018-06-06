using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocuda.Ops.Controllers.RouteConstraint;
using Ocuda.Ops.Data;
using Ocuda.Ops.Web.Middleware;
using Ops.Service;
using Serilog.Context;

namespace Ocuda.Ops.Web
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
            string culture = Configuration["Ops.Culture"] ?? "en-US";
            services.Configure<RequestLocalizationOptions>(_ =>
            {
                _.DefaultRequestCulture
                    = new Microsoft.AspNetCore.Localization.RequestCulture(culture);
            });

            services.AddDistributedMemoryCache();

            switch (Configuration["Ops.DatabaseProvider"])
            {
                case "SqlServer":
                    services.AddDbContextPool<OpsContext, DataProvider.SqlServer.Ops.Context>(_ =>
                        _.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;"));
                    services.AddDbContext<PromenadeContext, DataProvider.SqlServer.Promenade.Context>(_ =>
                        _.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;"));
                    break;
                case "SQLite":
                    services.AddDbContextPool<OpsContext, DataProvider.SQLite.Ops.Context>(_ =>
                        _.UseSqlite(@"ops.sqlite"));
                    services.AddDbContext<PromenadeContext, DataProvider.SQLite.Promenade.Context>(_ =>
                        _.UseSqlite(@"promenade.sqlite"));
                    break;
                default:
                    throw new System.Exception("No Ops.DatabaseProvider configured.");
            }

            services.AddSession(_ =>
            {
                _.IdleTimeout = System.TimeSpan.FromHours(2);
                _.Cookie.HttpOnly = true;
            });

            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            // filters
            services.AddScoped<Controllers.Filter.OpsFilter>();

            // services
            services.AddScoped<SectionService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // configure error page handling and development IDE linking
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute("/Error/Index/{0}");
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

            app.UseSession();

            app.UseOpsAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: null,
                    template: "{section}/{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" },
                    constraints: new
                    {
                        section = new SectionRouteConstraint()
                    });
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
