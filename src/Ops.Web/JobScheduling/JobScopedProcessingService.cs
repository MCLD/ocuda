using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;

namespace Ocuda.Ops.Web.JobScheduling
{
    internal class JobScopedProcessingService(ILogger<JobScopedProcessingService> logger,
        IDateTimeProvider dateTimeProvider,
        IDigitalDisplayCleanupService digitalDisplayCleanupService,
        IDigitalDisplaySyncService digitalDisplaySyncService,
        IEmediaReportingService emediaReportingService,
        IEmployeeCardReportingService employeeCardReportingService,
        IJobService jobService,
        IRenewCardReportingService renewCardReportingService,
        IScheduleNotificationService scheduleNotificationService,
        IUserManagementService userManagementService,
        IVolunteerNotificationService volunteerNotificationService)
        : BaseScopedBackgroundService<JobScopedProcessingService>(logger)
    {
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

            var adminUser = await userManagementService.EnsureSysadminUserAsync();

            var pendingJobs = await jobService.GetPendingJobsAsync();
            if (pendingJobs.Any())
            {
                foreach (var job in pendingJobs)
                {
                    try
                    {
                        job.Progress = new Progress<Job>();
                        job.UserId = adminUser.Id;

                        await RunJobAsync(job);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Critical error in job {JobId}: {ErrorMessage}",
                            job.Id,
                            ex.Message);
                    }
                }
            }

            try
            {
                await jobService.ScheduleJobsAsync(adminUser.Id);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Scheduling jobs failed: {ErrorMessage}", ex.Message);
            }

            _logger.LogDebug(
                "Scheduled tasks complete in {Elapsed} ms",
                StopProcessing().ElapsedMilliseconds);
        }

        private async Task RunJobAsync(Job job)
        {
            var jobDefinition = jobService.GetDefinition(job.JobType)
                ?? throw new OcudaException($"Job {job.Id} requested job type {job.JobType} which is is not defined.");

            job.PercentComplete = 0;
            job.StartedAt = dateTimeProvider.Now;
            job.Status = "Starting job.";

            try
            {
                // try/catch wrap this, if it fails we will never know the job started
                // if we can't start a job then it's a critical error
                await jobService.UpdateJobAsync(job);
            }
            catch (Exception ex)
            {
                throw new OcudaException(
                    $"Aborting running {job.Id} due to inability to mark it as started: {ex.Message}",
                    ex);
            }

            try
            {
                await jobDefinition.RunAsync(job, StatusUpdateAsync);

                job.FinishedAt = dateTimeProvider.Now;
                job.WasSuccessful = true;

                await jobService.UpdateJobAsync(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Critical error in job {JobId}: {ErrorMessage}",
                    job.Id,
                    ex.Message);
                job.Status = $"Critical error: {ex.Message}";
            }

            if (!job.WasSuccessful)
            {
                try
                {
                    job.FinishedAt = dateTimeProvider.Now;
                    job.WasSuccessful = false;

                    await StatusUpdateAsync(job);
                    await jobService.UpdateJobAsync(job);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(
                        ex,
                        "Unable to mark job {JobId} as errored, status will remain incomplete: {ErrorMessage}",
                        job.Id,
                        ex.Message);
                }
            }
        }

        private async Task StatusUpdateAsync(Job job)
        {
            job.Progress?.Report(job);
            await jobService.AddJobLogAsync(new JobLog
            {
                CreatedAt = dateTimeProvider.Now,
                CreatedBy = job.UserId,
                JobId = job.Id,
                PercentComplete = job.PercentComplete,
                Status = job.Status.TruncateTo(255),
            });
        }
    }
}
