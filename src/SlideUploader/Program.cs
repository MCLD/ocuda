using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ocuda.Utility.Exceptions;
using Serilog;

[assembly: CLSCompliant(true)]

namespace Ocuda.SlideUploader
{
    internal class Program
    {
        private const string DefaultJobFile = "job.json";
        private const string DefaultJobResultFile = "job-result.json";
        private const string Product = "Ocuda.SlideUploader";

        protected Program()
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Allow catching general exception at the top level of the app")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability",
            "CA1506:Avoid excessive class coupling",
            Justification = "Dependency injection leads to significant class coupling")]
        protected static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>()
                .AddCommandLine(args)
                .Build();

            Log.Logger = Utility.Logging.Configuration.Build(config).CreateLogger();

            var configSection = config.GetSection(SlideUploaderOptions.SlideUploader);
            var options = new SlideUploaderOptions();
            configSection.Bind(options);
            VerifyOptions(options);

            var authBaseUri = new Uri(options.AuthBase);

            var services = new ServiceCollection();
            services.Configure<SlideUploaderOptions>(_ => configSection.Bind(_));
            services.AddLogging(_ => _.AddSerilog());
            services.AddSingleton<Upload>();
            services.AddHttpClient<OpsClient>(_ => _.BaseAddress = new Uri(options.OpsBase))
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
                {
                    Credentials = new CredentialCache {
                        { authBaseUri, "NTLM", CredentialCache.DefaultNetworkCredentials }
                    }
                });

            var serviceProvider = services.BuildServiceProvider();

            var instance = string.IsNullOrEmpty(options.Instance) ? "n/a" : options.Instance;
            var version = Utility.Helpers.VersionHelper.GetVersion();

            try
            {
                Log.Verbose("{Product} v{Version} instance {Instance} starting up",
                    Product,
                    version,
                    instance);

                string jobFile = !string.IsNullOrEmpty(options.JobFile)
                    ? options.JobFile
                    : DefaultJobFile;

                string jobResultFile = !string.IsNullOrEmpty(options.JobResultFile)
                    ? options.JobResultFile
                    : DefaultJobResultFile;

                await serviceProvider
                    .GetRequiredService<Upload>()
                    .JobAsync(jobFile, jobResultFile);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "{Product} instance {Instance} v{Version} exited unexpectedly: {Message}",
                    Product,
                    instance,
                    version,
                    ex.Message);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void VerifyOptions(SlideUploaderOptions options)
        {
            var issues = new List<string>();

            if (options == null)
            {
                options = new SlideUploaderOptions();
                issues.Add("Configuration section 'SlideUploader' is not present in the configuration file");
            }

            if (string.IsNullOrEmpty(options.AuthBase))
            {
                issues.Add("Missing config: AuthBase - the base URL for authentication (e.g. 'https://authserver/auth')");
            }

            if (string.IsNullOrEmpty(options.OpsBase))
            {
                issues.Add("Missing config: OpsBase - the base URL to your Ops deployment");
            }

            if (issues.Count > 0)
            {
                Console.Error.WriteLine("Fatal configuration errors:");
                foreach (var issue in issues)
                {
                    Console.Error.WriteLine(issue);
                    Log.Fatal("Configuration error: {ConfigurationError}", issue);
                }

                throw new OcudaException("Aboring run: fatal configuration errors.");
            }
        }
    }
}