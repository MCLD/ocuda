using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocuda.Utility.Keys;
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

            var instance = string.IsNullOrEmpty(config[Configuration.OcudaInstance])
                ? "n/a"
                : config[Configuration.OcudaInstance];

            var version = Utility.Helpers.VersionHelper.GetVersion();

            Log.Logger = Utility.Logging.Configuration.Build(config).CreateLogger();
            Log.Information("{Product} v{Version} instance {Instance} starting up",
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
                if (string.IsNullOrEmpty(config[Configuration.OcudaRuntimeRedisCacheConfiguration]))
                {
                    Log.Information("{Product} distributed cache: in memory",
                        Product);
                }
                else
                {
                    Log.Information("{Product} distributed cache: Redis configuration {RedisConfiguration} instance {RedisInstance}",
                        Product,
                        config[Configuration.OcudaRuntimeRedisCacheConfiguration],
                        config[Configuration.OcudaRuntimeRedisCacheInstance]);
                }

                if (!string.IsNullOrEmpty(config[Configuration.OcudaRuntimeSessionTimeout]))
                {
                    Log.Information("{Product} session timeout configured for {SessionTimeout}",
                        Product,
                        config[Configuration.OcudaRuntimeSessionTimeout]);
                }

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
