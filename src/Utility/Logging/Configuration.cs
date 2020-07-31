using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Filters;

namespace Ocuda.Utility.Logging
{
    public static class Configuration
    {
        private const string DefaultErrorControllerName = "Controllers.ErrorController";

        public static LoggerConfiguration Build(IConfiguration config)
        {
            if (config == null)
            {
                throw new System.ArgumentNullException(nameof(config));
            }

            LoggerConfiguration loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.WithProperty(Enrichment.Application,
                    Assembly.GetExecutingAssembly().GetName().Name)
                .Enrich.WithProperty(Enrichment.Version, Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            string instance = config[Keys.Configuration.OcudaInstance];

            if (!string.IsNullOrEmpty(instance))
            {
                loggerConfig.Enrich.WithProperty(Enrichment.Instance, instance);
            }

            string errorControllerName = config[Keys.Configuration.OcudaErrorControllerName]
                ?? DefaultErrorControllerName;

            if (!string.IsNullOrEmpty(config[Keys.Configuration.OcudaLoggingRollingFile]))
            {
                string rollingLogLocation
                    = Path.Combine("shared", config[Keys.Configuration.OcudaLoggingRollingFile]);

                string rollingLogFile = !string.IsNullOrEmpty(instance)
                    ? Path.Combine(rollingLogLocation, $"log-{instance}-{{Date}}.txt")
                    : Path.Combine(rollingLogLocation, "log-{Date}.txt");

                loggerConfig.WriteTo.Logger(_ => _
                    .Filter.ByExcluding(Matching.FromSource(errorControllerName))
                    .WriteTo.RollingFile(rollingLogFile));

                string httpErrorFileTag = config[Keys.Configuration.OcudaLoggingRollingHttpFile];
                if (!string.IsNullOrEmpty(httpErrorFileTag))
                {
                    string httpLogFile = !string.IsNullOrEmpty(instance)
                        ? Path.Combine(rollingLogLocation,
                            $"{httpErrorFileTag}-{instance}-{{Date}}.txt")
                        : Path.Combine(rollingLogLocation + $"{httpErrorFileTag}-{{Date}}.txt");

                    loggerConfig.WriteTo.Logger(_ => _
                        .Filter.ByIncludingOnly(Matching.FromSource(errorControllerName))
                        .WriteTo.RollingFile(httpLogFile));
                }
            }

            string seqEndpoint = config[Keys.Configuration.OcudaSeqEndpoint];
            if (!string.IsNullOrEmpty(seqEndpoint))
            {
                loggerConfig
                    .WriteTo.Logger(_ => _
                        .Filter.ByExcluding(Matching.FromSource(errorControllerName))
                        .WriteTo.Seq(seqEndpoint,
                            apiKey: config[Keys.Configuration.OcudaSeqAPI]));
            }

            return loggerConfig;
        }
    }
}
