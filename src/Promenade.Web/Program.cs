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
        public static int Main(string[] args)
        {
            var webHost = CreateHostBuilder(args).Build();
            var config = (IConfiguration)webHost.Services.GetService(typeof(IConfiguration));

            Log.Logger = Utility.Logging.Configuration.Build(config).CreateLogger();

            var instanceConfig = config[Utility.Keys.Configuration.OcudaInstance];
            var instance = string.IsNullOrEmpty(instanceConfig) ? null : $" ({instanceConfig})";

            var version = Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion;

            // perform initialization
            using (IServiceScope scope = webHost.Services.CreateScope())
            {
                var web = new Web(scope);
                Task.Run(() => web.InitalizeAsync()).Wait();
            }

            string product = $"Ocuda.Promenade{instance}";

            try
            {
                Log.Information("{Product} v{Version} starting up", product, version);
                webHost.Run();
                return 0;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                Log.Fatal(ex, "{Product} v{Version} exited unexpectedly: {Message}",
                    product,
                    version,
                    ex.Message);
                return 1;
            }
#pragma warning restore CA1031 // Do not catch general exception types
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .UseSerilog();
    }
}
