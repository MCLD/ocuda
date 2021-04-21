using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Ocuda.Utility.Keys;
using Serilog;

namespace Ops.Web.WindowsAuth
{
    public static class Program
    {
        private const string Product = "Ocuda.Ops.Web.WindowsAuth";

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .UseSerilog();


        public static int Main(string[] args)
        {
            using var webHost = CreateHostBuilder(args).Build();
            var config = (IConfiguration)webHost.Services.GetService(typeof(IConfiguration));

            var instance = string.IsNullOrEmpty(config[Configuration.OcudaInstance])
                ? "n/a"
                : config[Configuration.OcudaInstance];

            var version = Ocuda.Utility.Helpers.VersionHelper.GetVersion();

            Log.Logger = Ocuda.Utility.Logging.Configuration.Build(config).CreateLogger();
            Log.Information("{Product} v{Version} instance {Instance} starting up",
                Product,
                version,
                instance);

            try
            {
                webHost.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "{Product} instance {Instance} v{Version} exited unexpectedly: {Message}",
                    Product,
                    instance,
                    version,
                    ex.Message);
                throw;
            }
            finally
            {
                Log.Information("{Product} instance {Instance} v{Version} shutting down",
                   Product,
                   instance,
                   version);
                Log.CloseAndFlush();
            }
        }
    }
}
