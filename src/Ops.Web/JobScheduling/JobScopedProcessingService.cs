using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Web.JobScheduling
{
    internal class JobScopedProcessingService
        : BaseScopedBackgroundService<JobScopedProcessingService>
    {
        private readonly IDigitalDisplaySyncService _digitalDisplaySyncService;
        private readonly IScheduleNotificationService _scheduleNotificationService;

        public JobScopedProcessingService(ILogger<JobScopedProcessingService> logger,
            IDigitalDisplaySyncService digitalDisplaySyncService,
            IScheduleNotificationService scheduleNotificationService)
            : base(logger)
        {
            _digitalDisplaySyncService = digitalDisplaySyncService
                ?? throw new ArgumentNullException(nameof(digitalDisplaySyncService));
            _scheduleNotificationService = scheduleNotificationService
                ?? throw new ArgumentNullException(nameof(scheduleNotificationService));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Catch all exceptions and log them as this runs headless")]
        public override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartProcessing();

            try
            {
                await Task.WhenAll(
                    _scheduleNotificationService.SendPendingNotificationsAsync(),
                    _digitalDisplaySyncService.UpdateDigitalDisplaysAsync());
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Error running scheduled task: {ErrorMessage}",
                    ex.Message);
            }

            _logger.LogDebug("Scheduled tasks complete in {Elapsed} ms",
                StopProcessing().ElapsedMilliseconds);
        }
    }
}