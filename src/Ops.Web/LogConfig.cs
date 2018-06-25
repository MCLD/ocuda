using System;
using System.Data;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.MSSqlServer;

namespace Ocuda.Ops.Web
{
    public class LogConfig
    {
        private const string ErrorControllerName = "Ocuda.Ops.Controllers.ErrorController";

        private const string ApplicationEnrichment = "Application";
        private const string VersionEnrichment = "Version";
        private const string IdentifierEnrichment = "Identifier";
        private const string InstanceEnrichment = "Instance";
        private const string RemoteAddressEnrichment = "RemoteAddress";

        public LoggerConfiguration Build(string applicationName, 
            Version version, 
            int id,
            string[] args)
        {
            var config = (IConfiguration)WebHost
                .CreateDefaultBuilder(args)
                .Build()
                .Services
                .GetService(typeof(IConfiguration));

            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .ReadFrom.Configuration(config)
                .Enrich.WithProperty(ApplicationEnrichment, applicationName)
                .Enrich.WithProperty(VersionEnrichment, version)
                .Enrich.WithProperty(IdentifierEnrichment, id)
                .Enrich.FromLogContext()
                .WriteTo.Console();

            var instance = config[Utility.Keys.Configuration.OpsInstance] ?? "ops";
            if (!string.IsNullOrEmpty(instance))
            {
                loggerConfig.Enrich.WithProperty(InstanceEnrichment, instance);
            }

            var rollingLogLocation = config[Utility.Keys.Configuration.OpsRollingLogLocation];
            if (!string.IsNullOrEmpty(rollingLogLocation))
            {
                if (!rollingLogLocation.EndsWith(Path.DirectorySeparatorChar))
                {
                    rollingLogLocation += Path.DirectorySeparatorChar;
                }
                rollingLogLocation += "log-";

                string rollingLogFile = rollingLogLocation + instance + "-{Date}.txt";

                loggerConfig.WriteTo.Logger(_ => _
                    .Filter.ByExcluding(Matching.FromSource(ErrorControllerName))
                    .WriteTo.RollingFile(rollingLogFile));

                string httpErrorFileTag = config[Utility.Keys.Configuration.OpsHttpErrorFileTag];
                if (!string.IsNullOrEmpty(httpErrorFileTag))
                {
                    string httpLogFile = rollingLogLocation 
                        + instance 
                        + "-"
                        + httpErrorFileTag 
                        + "-{Date}.txt";

                    loggerConfig.WriteTo.Logger(_ => _
                        .Filter.ByIncludingOnly(Matching.FromSource(ErrorControllerName))
                        .WriteTo.RollingFile(httpLogFile));
                }
            }

            var sqlSoftwareLogCs = config.GetConnectionString("SerilogSoftwareLogs");
            if (!string.IsNullOrEmpty(sqlSoftwareLogCs))
            {
                loggerConfig
                    .WriteTo.Logger(_ => _
                    .Filter.ByExcluding(Matching.FromSource(ErrorControllerName))
                    .WriteTo.MSSqlServer(sqlSoftwareLogCs,
                        "Logs",
                        autoCreateSqlTable: true,
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        columnOptions: new ColumnOptions
                        {
                            AdditionalDataColumns = new DataColumn[]
                            {
                                new DataColumn(ApplicationEnrichment, typeof(string)),
                                new DataColumn(VersionEnrichment, typeof(string)),
                                new DataColumn(IdentifierEnrichment, typeof(int)),
                                new DataColumn(InstanceEnrichment, typeof(string)),
                                new DataColumn(RemoteAddressEnrichment, typeof(string))
                            }
                        }));
            }

            return loggerConfig;
        }
    }
}
