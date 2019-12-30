using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Ocuda.Ops.Web
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var webHost = CreateHostBuilder(args).Build();
            var config = (IConfiguration)webHost.Services.GetService(typeof(IConfiguration));

            Log.Logger = new Utility.Logging.Configuration().Build(config).CreateLogger();

            var instanceConfig = config[Utility.Keys.Configuration.OcudaInstance];
            var instance = string.IsNullOrEmpty(instanceConfig) ? null : $" ({instanceConfig})";

            var version = Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion;

            try
            {
                Log.Information($"Ocuda.Ops{instance} v{version} starting up");
                webHost.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"Ocuda.Ops{instance} v{version} exited unexpectedly: {ex.Message}");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();
    }
}
