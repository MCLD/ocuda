using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Ocuda.Utility.Keys;
using Serilog;
using Serilog.Events;

namespace Ops.Web.WindowsAuth
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var applicationName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            string logPath = "logs";
            string logFile = "log-{Date}.txt";

            // build a WebHost so that we can access configuration
            IWebHost webHost = CreateWebHostBuilder(args).Build();
            var config = (IConfiguration)webHost.Services.GetService(typeof(IConfiguration));

            if (!string.IsNullOrEmpty(config[Configuration.OcudaLoggingRollingFile]))
            {
                logPath = config[Configuration.OcudaLoggingRollingFile];
                if (!System.IO.Directory.Exists(logPath))
                {
                    System.IO.Directory.CreateDirectory(logPath);
                }
            }

            string fullLogPath = System.IO.Path.Combine(logPath, logFile);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                .MinimumLevel.Override("System", LogEventLevel.Error)
                .ReadFrom.Configuration(config)
                .Enrich.WithProperty("Application", applicationName)
                .Enrich.WithProperty("Version", version)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(fullLogPath)
                .CreateLogger();

            // rebuild WebHost now that we have logging configuration
            webHost = CreateWebHostBuilder(args).Build();

            try
            {
                Log.Information($"{applicationName} v{version} starting up");
                webHost.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"{applicationName} v{version} exited unexpectedly: {ex.Message}");
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
