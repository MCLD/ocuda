using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocuda.Utility.Keys;
using Serilog;

namespace Ocuda.Ops.Web
{
    public static class Program
    {
        private const string Product = "Ocuda.Ops";

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .UseSerilog();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Handle top-level system exceptions properly")]
        public static int Main(string[] args)
        {
            using var webHost = CreateHostBuilder(args).Build();
            var config = (IConfiguration)webHost.Services.GetService(typeof(IConfiguration));

            var instance = string.IsNullOrEmpty(config[Configuration.OcudaInstance])
                ? "n/a"
                : config[Configuration.OcudaInstance];

            var version = Utility.Helpers.VersionHelper.GetVersion();

            Log.Logger = Utility.Logging.Configuration.Build(config).CreateLogger();
            Log.Information("{Product} v{Version} instance {Instance} starting up",
                Product,
                version,
                instance);

            try
            {
                if (string.IsNullOrEmpty(config[Configuration.OcudaRuntimeRedisCacheConfiguration]))
                {
                    Log.Information("Distributed cache: in memory");
                }
                else
                {
                    Log.Information("Distributed cache: Redis configuration {RedisConfiguration} instance {RedisInstance}",
                        config[Configuration.OcudaRuntimeRedisCacheConfiguration],
                        config[Configuration.OcudaRuntimeRedisCacheInstance]);
                }

                if (!string.IsNullOrEmpty(config[Configuration.OcudaRuntimeSessionTimeout]))
                {
                    Log.Information("{Product} session timeout configured for {SessionTimeout}",
                        Product,
                        config[Configuration.OcudaRuntimeSessionTimeout]);
                }

                using (var scope = webHost.Services.CreateScope())
                {
                    Task.Run(() => new CacheManagement(scope).StartupClearAsync()).Wait();
                }

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
                return 1;
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