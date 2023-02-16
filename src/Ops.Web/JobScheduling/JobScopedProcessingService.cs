using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Web.JobScheduling
{
    internal class JobScopedProcessingService
        : BaseScopedBackgroundService<JobScopedProcessingService>
    {
        private readonly IDigitalDisplayCleanupService _digitalDisplayCleanupService;
        private readonly IDigitalDisplaySyncService _digitalDisplaySyncService;
        private readonly IScheduleNotificationService _scheduleNotificationService;

        public JobScopedProcessingService(ILogger<JobScopedProcessingService> logger,
            IDigitalDisplayCleanupService digitalDisplayCleanupService,
            IDigitalDisplaySyncService digitalDisplaySyncService,
            IScheduleNotificationService scheduleNotificationService)
            : base(logger)
        {
            _digitalDisplayCleanupService = digitalDisplayCleanupService
                ?? throw new ArgumentNullException(nameof(digitalDisplayCleanupService));
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

            var scheduledTasks = new Dictionary<string, Func<Task>>
            {
                ["SendPendingNotifications"] = _scheduleNotificationService.SendPendingNotificationsAsync,
                ["CleanupSlides"] = _digitalDisplayCleanupService.CleanupSlidesAsync,
                ["UpdateDigitalDisplays"] = _digitalDisplaySyncService.UpdateDigitalDisplaysAsync
            };

            foreach (var methodName in scheduledTasks.Keys)
            {
                try
                {
                    await scheduledTasks[methodName]();
                }
                catch (Exception ex)
                {
                    int preventLoop = 10;
                    var innerException = ex;
                    using (_logger.BeginScope(new Dictionary<string, object>
                    {
                        ["TopException"] = ex
                    }))
                    {
                        while (innerException.InnerException != null && preventLoop > 0)
                        {
                            innerException = innerException.InnerException;
                            preventLoop--;
                        }
                        _logger.LogCritical(ex,
                            "Critical error in scheduled task {MethodName}: {ErrorMessage}",
                            methodName,
                            innerException.Message);
                    }
                }
            }

            _logger.LogDebug("Scheduled tasks complete in {Elapsed} ms",
                StopProcessing().ElapsedMilliseconds);
        }
    }
}