using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Web.JobScheduling
{
    internal abstract class BaseScopedBackgroundService<T> where T : class
    {
        protected readonly Stopwatch _sw;
        protected readonly ILogger _logger;

        protected BaseScopedBackgroundService(ILogger<T> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _sw = new Stopwatch();
        }

        public abstract Task ExecuteAsync(CancellationToken stoppingToken);

        protected void StartProcessing()
        {
            _sw.Reset();
            _sw.Start();
        }

        protected Stopwatch StopProcessing()
        {
            if (_sw?.IsRunning == true)
            {
                _sw.Stop();
            }
            return _sw;
        }
    }
}
