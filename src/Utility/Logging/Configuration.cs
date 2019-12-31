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

        public static readonly string ApplicationEnrichment = "Application";
        public static readonly string VersionEnrichment = "Version";
        public static readonly string IdentifierEnrichment = "Identifier";
        public static readonly string InstanceEnrichment = "Instance";
        public static readonly string RemoteAddressEnrichment = "RemoteAddress";

        public static LoggerConfiguration Build(IConfiguration config)
        {
            if(config == null)
            {
                throw new System.ArgumentNullException(nameof(config));
            }

            LoggerConfiguration loggerConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .Enrich.WithProperty(ApplicationEnrichment,
                    Assembly.GetExecutingAssembly().GetName().Name)
                .Enrich.WithProperty(VersionEnrichment, Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            string instance = config[Keys.Configuration.OcudaInstance];

            if (!string.IsNullOrEmpty(instance))
            {
                loggerConfig.Enrich.WithProperty(InstanceEnrichment, instance);
            }

            string errorControllerName = config[Keys.Configuration.OcudaErrorControllerName]
                ?? DefaultErrorControllerName;

            string rollingLogLocation
                = Path.Combine("shared", config[Keys.Configuration.OcudaLoggingRollingFile]);
            if (!string.IsNullOrEmpty(rollingLogLocation))
            {
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

            string sqlLog = config.GetConnectionString(Keys.Configuration.OcudaLoggingDatabase);
            if (!string.IsNullOrEmpty(sqlLog))
            {
                loggerConfig
                    .WriteTo.Logger(_ => _
                    .Filter.ByExcluding(Matching.FromSource(errorControllerName))
                    .WriteTo.MSSqlServer(sqlLog,
                        "Logs",
                        autoCreateSqlTable: true,
                        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                        columnOptions: new Serilog.Sinks.MSSqlServer.ColumnOptions
                        {
                            AdditionalColumns = new []
                            {
                                new Serilog.Sinks.MSSqlServer.SqlColumn(ApplicationEnrichment,
                                    System.Data.SqlDbType.NVarChar) { DataLength = 255 },
                                new Serilog.Sinks.MSSqlServer.SqlColumn(VersionEnrichment,
                                    System.Data.SqlDbType.NVarChar) { DataLength = 255 },
                                new Serilog.Sinks.MSSqlServer.SqlColumn(IdentifierEnrichment,
                                    System.Data.SqlDbType.NVarChar) { DataLength = 255 },
                                new Serilog.Sinks.MSSqlServer.SqlColumn(InstanceEnrichment,
                                    System.Data.SqlDbType.NVarChar) { DataLength = 255 },
                                new Serilog.Sinks.MSSqlServer.SqlColumn(RemoteAddressEnrichment,
                                    System.Data.SqlDbType.NVarChar) { DataLength = 255 }
                            }
                        }));
            }

            string seqEndpoint = config[Keys.Configuration.OcudaSeqEndpoint];
            if(!string.IsNullOrEmpty(seqEndpoint))
            {
                loggerConfig
                    .WriteTo.Logger(_ => _
                        .Filter.ByExcluding(Matching.FromSource(errorControllerName))
                        .WriteTo.Seq(seqEndpoint));
            }

            return loggerConfig;
        }
    }
}
