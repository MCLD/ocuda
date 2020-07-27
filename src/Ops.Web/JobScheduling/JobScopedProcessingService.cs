﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Web.JobScheduling
{
    internal class JobScopedProcessingService
        : BaseScopedBackgroundService<JobScopedProcessingService>
    {
        private readonly IScheduleNotificationService _scheduleNotificationService;

        public JobScopedProcessingService(ILogger<JobScopedProcessingService> logger,
            IScheduleNotificationService scheduleNotificationService)
            : base(logger)
        {
            _scheduleNotificationService = scheduleNotificationService
                ?? throw new ArgumentNullException(nameof(scheduleNotificationService));
        }

        public async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartProcessing();

            int sent = await _scheduleNotificationService.SendPendingNotificationsAsync();

            if (sent > 0)
            {
                _logger.LogDebug("Scheduled tasks sent {SentNotifications} email in {Elapsed} ms",
                    sent,
                    StopProcessing().ElapsedMilliseconds);
            }
            else
            {
                _logger.LogDebug("Scheduled tasks complete in {Elapsed} ms",
                    StopProcessing().ElapsedMilliseconds);
            }
        }
    }
}
