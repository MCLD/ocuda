using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Web.JobScheduling
{
    public class JobBackgroundService : BackgroundService
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly IServiceProvider _services;

        private readonly string InstanceName;
        private readonly bool JobsEnabled;
        private readonly int JobSleepSeconds;

        public JobBackgroundService(IDistributedCache cache,
            IConfiguration config,
            ILogger<JobBackgroundService> logger,
            IServiceProvider services)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _services = services ?? throw new ArgumentNullException(nameof(services));

            InstanceName = string.IsNullOrEmpty(_config[Configuration.OcudaInstance])
                ? "n/a"
                : _config[Configuration.OcudaInstance];

            if (!int.TryParse(_config[Configuration.OcudaJobSleepSeconds],
                out JobSleepSeconds))
            {
                _logger.LogInformation("No Ocuda.JobSleep configured in {InstanceName}, not running background jobs.",
                    InstanceName);
                JobsEnabled = false;
            }
            else
            {
                JobsEnabled = true;
            }
        }

        private async Task<bool> GetCancellationOrder(int seconds)
        {
            var cancellation = await _cache
                .GetStringAsync(string.Format(Cache.OpsJobStop, InstanceName));

            if (cancellation == InstanceName)
            {
                await _cache.RemoveAsync(string.Format(Cache.OpsJobStop, InstanceName));
                _logger.LogInformation("Cancellation received for job service {InstanceName}",
                    InstanceName);
                return true;
            }

            await _cache.SetStringAsync(Cache.OpsJobRunner,
                InstanceName,
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromSeconds(seconds + 10)
                });

            return false;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!JobsEnabled)
            {
                return;
            }

            await _cache.RemoveAsync(string.Format(Cache.OpsJobStop, InstanceName));

            _logger.LogDebug("Starting job service in instance {InstanceName} with a sleep value of {JobSleep} s",
                InstanceName,
                JobSleepSeconds);

            if (!await GetCancellationOrder(JobSleepSeconds))
            {
                await Task.Delay(JobSleepSeconds * 1000, stoppingToken);

                while (!stoppingToken.IsCancellationRequested
                    && !await GetCancellationOrder(JobSleepSeconds))
                {
                    using (var scope = _services.CreateScope())
                    {
                        await scope.ServiceProvider
                                .GetRequiredService<JobScopedProcessingService>()
                                .ExecuteAsync(stoppingToken);
                    }

                    if (await GetCancellationOrder(JobSleepSeconds))
                    {
                        break;
                    }
                    else
                    {
                        await Task.Delay(JobSleepSeconds * 1000, stoppingToken);
                    }
                }
            }

            _logger.LogInformation("Ending job service in instance {InstanceName}",
                InstanceName);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Stopping job service in instance {InstanceName}",
                string.IsNullOrEmpty(_config[Configuration.OcudaInstance])
                    ? "n/a"
                    : _config[Configuration.OcudaInstance]);

            await _cache.SetStringAsync(string.Format(Cache.OpsJobStop, InstanceName),
                InstanceName,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
        }
    }
}
