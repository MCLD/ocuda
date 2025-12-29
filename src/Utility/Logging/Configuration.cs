using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Filters;

namespace Ocuda.Utility.Logging
{
    public static class Configuration
    {
        public static LoggerConfiguration Build(IConfiguration config)
        {
            System.ArgumentNullException.ThrowIfNull(config);

            LoggerConfiguration loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.WithProperty(Enrichment.Application,
                    Assembly.GetExecutingAssembly().GetName().Name)
                .Enrich.WithProperty(Enrichment.Version, Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion)
                .Enrich.FromLogContext();

            string instance = config[Keys.Configuration.OcudaInstance];

            if (!string.IsNullOrEmpty(instance))
            {
                loggerConfig.Enrich.WithProperty(Enrichment.Instance, instance);
            }

            return loggerConfig;
        }
    }
}