﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocuda.Ops.Controllers.RouteConstraint;
using Ocuda.Ops.Controllers.Validator;
using Ocuda.Ops.Data;
using Ocuda.Ops.Service;
using Ocuda.Ops.Web.Middleware;
using Ocuda.Ops.Web.StartupHelper;

namespace Ocuda.Ops.Web
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration configuration)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var logger = Serilog.Log.Logger;
            // set a default culture of en-US if none is specified
            string culture = _config["Ops.Culture"] ?? "en-US";
            logger.Information($"Configuring for culture: {culture}");
            services.Configure<RequestLocalizationOptions>(_ =>
            {
                _.DefaultRequestCulture
                    = new Microsoft.AspNetCore.Localization.RequestCulture(culture);
            });

            services.AddDistributedMemoryCache();

            switch (_config["Ops.DatabaseProvider"])
            {
                case "SqlServer":
                    logger.Information("Using SqlServer data provider");
                    services.AddDbContextPool<OpsContext, DataProvider.SqlServer.Ops.Context>(_ =>
                        _.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Database=Ocuda.Ops;Trusted_Connection=True;MultipleActiveResultSets=True"));
                    services.AddDbContextPool<PromenadeContext, DataProvider.SqlServer.Promenade.Context>(_ =>
                        _.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Database=Ocuda.Promenade;Trusted_Connection=True;MultipleActiveResultSets=True"));
                    break;
                case "SQLite":
                    logger.Information("Using SQLite data provider");
                    services.AddDbContextPool<OpsContext, DataProvider.SQLite.Ops.Context>(_ =>
                        _.UseSqlite(@"Data Source=ops.db"));
                    services.AddDbContextPool<PromenadeContext, DataProvider.SQLite.Promenade.Context>(_ =>
                        _.UseSqlite(@"Data Source=promenade.db"));
                    break;
                default:
                    logger.Fatal("No Ops.DatabaseProvider configured in settings. Exiting.");
                    throw new Exception("No Ops.DatabaseProvider configured.");
            }

            services.AddSession(_ =>
            {
                _.IdleTimeout = TimeSpan.FromHours(2);
                _.Cookie.HttpOnly = true;
            });

            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            // filters
            services.AddScoped<Controllers.Filter.OpsFilter>();

            // repositories
            services.AddScoped<Service.Interfaces.Ops.IFileRepository, Data.Ops.FileRepository>();
            services.AddScoped<Service.Interfaces.Ops.ILinkRepository, Data.Ops.LinkRepository>();
            services.AddScoped<Service.Interfaces.Ops.ICategoryRepository, Data.Ops.CategoryRepository>();
            services.AddScoped<Service.Interfaces.Ops.IPageRepository, Data.Ops.PageRepository>();
            services.AddScoped<Service.Interfaces.Ops.IPostRepository, Data.Ops.PostRepository>();
            services.AddScoped<Service.Interfaces.Ops.ISectionRepository, Data.Ops.SectionRepository>();
            services.AddScoped<Service.Interfaces.Ops.ISiteSettingRepository, Data.Ops.SiteSettingRepository>();
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

            app.UseOpsAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: null,
                    template: "{area:exists}/{section}/{controller}/{action}/{id?}",
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
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
