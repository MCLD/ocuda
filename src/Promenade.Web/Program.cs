using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Ocuda.Promenade.Web
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var webHost = CreateWebHostBuilder(args).Build();
            var config = (IConfiguration)webHost.Services.GetService(typeof(IConfiguration));

            Log.Logger = new Utility.Logging.Configuration().Build(config).CreateLogger();

            var instanceConfig = config[Utility.Keys.Configuration.OcudaInstance];
            var instance = string.IsNullOrEmpty(instanceConfig) ? null : $" ({instanceConfig})";

            var version = Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion;

            try
            {
                Log.Information($"Ocuda.Promenade{instance} v{version} starting up");
                webHost.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, 
                    $"Ocuda.Promenade{instance} v{version} exited unexpectedly: {ex.Message}");
                return 1;
            }
            finally
            {

                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog();
    }
}
