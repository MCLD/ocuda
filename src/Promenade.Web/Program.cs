using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Ocuda.Promenade.Web
{
    public static class Program
    {
        private const string Product = "Ocuda.Promenade";

        public static int Main(string[] args)
        {
            using var webHost = CreateHostBuilder(args).Build();
            var config = (IConfiguration)webHost.Services.GetService(typeof(IConfiguration));

            var instance = string.IsNullOrEmpty(config[Utility.Keys.Configuration.OcudaInstance])
                ? "n/a"
                : config[Utility.Keys.Configuration.OcudaInstance];

            var version = Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion;

            Log.Logger = Utility.Logging.Configuration.Build(config).CreateLogger();
            Log.Warning("{Product} v{Version} instance {Instance} starting up",
                Product,
                version,
                instance);

            // perform initialization
            using (IServiceScope scope = webHost.Services.CreateScope())
            {
                Task.Run(() => new Web(scope).InitalizeAsync()).Wait();
            }

            try
            {
                webHost.Run();
                return 0;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                Log.Fatal(ex, "{Product} instance {Instance} v{Version} exited unexpectedly: {Message}",
                    Product,
                    instance,
                    version,
                    ex.Message);
                return 1;
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                Log.Information("{Product} instance {Instance} v{Version} shutting down",
                   Product,
                   instance,
                   version);
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .UseSerilog();
    }
}
