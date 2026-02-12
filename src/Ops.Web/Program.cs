using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Ocuda.Utility.Keys;
using Serilog;

namespace Ocuda.Ops.Web
{
    public static class Program
    {
        private const string EnvAspNetCoreEnv = "ASPNETCORE_ENVIRONMENT";
        private const string EnvRunningInContainer = "DOTNET_RUNNING_IN_CONTAINER";
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

            new Web(webHost.Services).Initalize();

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

                Task.Run(() => new CacheManagement(webHost.Services).StartupClearAsync()).Wait();

                webHost.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "{Product} v{Version} instance {Instance} exited unexpectedly: {Message}",
                    Product,
                    version,
                    instance,
                    ex.Message);
                return 1;
            }
            finally
            {
                Log.Information("{Product} v{Version} instance {Instance} shutting down",
                   Product,
                   version,
                   instance);
                Log.CloseAndFlush();
            }
        }
    }
}