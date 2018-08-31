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
    public class Program
    {
        public static int Main(string[] args)
        {
            var applicationName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            string logPath = "logs";
            string logFile = "log-{Date}.txt";
            IWebHost webHost = CreateWebHostBuilder(args).Build();
            var config = (IConfiguration)webHost.Services.GetService(typeof(IConfiguration));

            if (!string.IsNullOrEmpty(config[Configuration.OpsRollingLogLocation]))
            {
                logPath = config[Configuration.OpsRollingLogLocation];
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
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.RollingFile(fullLogPath)
                .CreateLogger();

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
