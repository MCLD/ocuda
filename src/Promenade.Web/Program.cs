using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Ocuda.Utility.Keys;
using Serilog;

namespace Ocuda.Promenade.Web
{
    public static class Program
    {
        private const string EnvAspNetCoreEnv = "ASPNETCORE_ENVIRONMENT";
        private const string EnvRunningInContainer = "DOTNET_RUNNING_IN_CONTAINER";
        private const string Product = "Ocuda.Promenade";

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
                .UseSerilog();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Catch exceptions at the top level of the application.")]
        public static int Main(string[] args)
        {
            using var webHost = CreateHostBuilder(args).Build();
            var config = (IConfiguration)webHost.Services.GetService(typeof(IConfiguration));

            var instance = string.IsNullOrEmpty(config[Configuration.OcudaInstance])
                ? "n/a"
                : config[Configuration.OcudaInstance];

            var version = Utility.Helpers.VersionHelper.GetVersion();

            var webHostEnvironment
                = (IWebHostEnvironment)webHost.Services.GetService(typeof(IWebHostEnvironment));

            Log.Logger = Utility.Logging.Configuration.Build(config).CreateLogger();
            Log.Information("{Product} v{Version} instance {Instance} environment {Environment} in {WebRootPath} with content root {ContentRoot} starting up",
                Product,
                version,
                instance,
                config[EnvAspNetCoreEnv] ?? "Production",
                webHostEnvironment.WebRootPath,
                config[Configuration.OcudaUrlSharedContent]);

            if (!string.IsNullOrEmpty(config[EnvRunningInContainer]))
            {
                Log.Information("Containerized: commit {ContainerCommit} created on {ContainerDate} image {ContainerImageVersion}",
                    config["org.opencontainers.image.revision"] ?? "unknown",
                    config["org.opencontainers.image.created"] ?? "unknown",
                    config["org.opencontainers.image.version"] ?? "unknown");
            }

            // perform initialization
            Task.Run(() => new Web(webHost.Services).InitalizeAsync()).Wait();

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