using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Web.JobScheduling
{
    internal class JobScopedProcessingService(ILogger<JobScopedProcessingService> logger,
        IDigitalDisplayCleanupService digitalDisplayCleanupService,
        IDigitalDisplaySyncService digitalDisplaySyncService,
        IEmediaReportingService emediaReportingService,
        IEmployeeCardReportingService employeeCardReportingService,
        IRenewCardReportingService renewCardReportingService,
        IScheduleNotificationService scheduleNotificationService,
        IVolunteerNotificationService volunteerNotificationService)
        : BaseScopedBackgroundService<JobScopedProcessingService>(logger)
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Catch all exceptions and log them as this runs headless")]
        public override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartProcessing();

            var scheduledTasks = new Dictionary<string, Func<Task>>
            {
                ["SendPendingScheduleNotifications"]
                    = scheduleNotificationService.SendPendingNotificationsAsync,
                ["CleanupSlides"] = digitalDisplayCleanupService.CleanupSlidesAsync,
                ["UpdateDigitalDisplays"] = digitalDisplaySyncService.UpdateDigitalDisplaysAsync,
                ["SendPendingVolunteerNotifications"]
                    = volunteerNotificationService.SendPendingNotificationsAsync,
                ["OnlineCardRenewalReports"] = renewCardReportingService.RunPendingReportsAsync,
                ["EmediaAccessReports"] = emediaReportingService.RunPendingReportsAsync,
                ["EmployeeCardReports"] = employeeCardReportingService.RunPendingReportsAsync,
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
                        ["TopException"] = ex,
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