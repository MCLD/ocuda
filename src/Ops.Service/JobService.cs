using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;

namespace Ocuda.Ops.Service
{
    public class JobService(
        ILogger<JobService> logger,
        IHttpContextAccessor httpContextAccessor,
        IDateTimeProvider dateTimeProvider,
        IJobConfigurationRepository jobConfigurationRepository,
        IJobLogRepository jobLogRepository,
        IJobRepository jobRepository,
        IUserSyncService userSyncService)
        : BaseService<JobService>(logger, httpContextAccessor),
        IJobService
    {
        public async Task AddJobLogAsync(JobLog jobLogs)
        {
            await jobLogRepository.AddAsync(jobLogs);
            await jobLogRepository.SaveAsync();
        }

        public async Task EnsureJobConfigurationsAsync(int userId)
        {
            var definitions = GetAllDefinitions().ToList();
            var typesPresent = await jobConfigurationRepository.GetAllTypes();

            var missingTypes = definitions.Select(_ => _.JobType).Except(typesPresent);

            if (missingTypes.Any())
            {
                foreach (var missingType in missingTypes)
                {
                    var jobDefinition = definitions.Single(_ => _.JobType == missingType);

                    _logger.LogInformation(
                        "Adding missing job definition to database: {Name}",
                        jobDefinition.Name);

                    await jobConfigurationRepository.AddAsync(new JobConfiguration
                    {
                        AutomaticallySchedule = jobDefinition.DefaultAutomaticallySchedule,
                        CanRunManually = jobDefinition.DefaultCanRunManually,
                        CreatedAt = dateTimeProvider.Now,
                        CreatedBy = userId,
                        MinimumSecondsBetweenRuns = jobDefinition.DefaultMinimumSecondsBetweenRuns,
                    });

                    await jobConfigurationRepository.SaveAsync();
                }
            }
        }

        public JobDefinition GetDefinition(JobType jobType)
        {
            return GetAllDefinitions().Single(_ => _.JobType == jobType);
        }

        public async Task<IEnumerable<Job>> GetPendingJobsAsync()
        {
            return await jobRepository.GetPendingAsync();
        }

        public async Task ScheduleJobsAsync(int userId)
        {
            var configurations = await jobConfigurationRepository.GetAutomaticallyScheduleAsync();

            if (configurations.Any())
            {
                var pending = await GetPendingJobsAsync();

                logger.LogDebug(
                    "Found {Count} potential jobs to schedule and {PendingCount} pending to run",
                    configurations.Count(),
                    pending.Count());

                var pendingTypes = pending.Select(_ => _.JobType);
                var notPending = configurations.Where(_ => !pendingTypes.Contains(_.Id));

                foreach (var configuration in notPending)
                {
                    bool schedule = true;

                    logger.LogDebug(
                        "Found {JobType} is not pending and can potentially be scheduled",
                        configuration.Id);

                    if (configuration.MinimumSecondsBetweenRuns.HasValue)
                    {
                        var lastRun = await GetLastCompletedFinishedTime(configuration.Id);

                        var nextRunEligible = lastRun
                            .FinishedAt
                            .Value
                            .AddSeconds(configuration.MinimumSecondsBetweenRuns.Value);

                        if (dateTimeProvider.Now < nextRunEligible)
                        {
                            schedule = false;
                            logger.LogDebug("Now ({Now}) < last run ({LastRun}) + minimum elapsed ({MinimumElapsed}) can NOT schedule",
                                dateTimeProvider.Now,
                                lastRun.StartedAt,
                                configuration.MinimumSecondsBetweenRuns);
                        }
                        else
                        {
                            schedule = true;
                            logger.LogDebug("Now ({Now}) is >= last run ({LastRun}) + minimum elapsed ({MinimumElapsed}) can schedule",
                                dateTimeProvider.Now,
                                lastRun.StartedAt,
                                configuration.MinimumSecondsBetweenRuns);
                        }
                    }

                    if (schedule)
                    {
                        logger.LogDebug("Scheduling job {JobType}", configuration.Id);
                        await jobRepository.AddAsync(new Job
                        {
                            CreatedAt = dateTimeProvider.Now,
                            CreatedBy = userId,
                            JobType = configuration.Id,
                        });
                        await jobRepository.SaveAsync();
                    }
                }
            }
        }

        public async Task UpdateJobAsync(Job job)
        {
            var updateJob = await jobRepository.FindAsync(job.Id);
            updateJob.CancelledAt = job.CancelledAt;
            updateJob.FinishedAt = job.FinishedAt;
            updateJob.StartedAt = job.StartedAt;
            updateJob.UpdatedAt = dateTimeProvider.Now;
            updateJob.UpdatedBy = job.UserId;
            updateJob.WasSuccessful = job.WasSuccessful;

            jobRepository.Update(updateJob);
            await jobRepository.SaveAsync();
        }

        private IEnumerable<JobDefinition> GetAllDefinitions()
        {
            return [
                new()
                    {
                        DefaultAutomaticallySchedule = false,
                        DefaultCanRunManually = true,
                        DefaultMinimumSecondsBetweenRuns = 60 * 60 * 12,
                        Description = "Synchronize user list with Active Directory",
                        JobType = JobType.SyncUsers,
                        Name = "Sync Users",
                        RunAsync = userSyncService.JobSyncDirectoryAsync,
                    },
            ];
        }

        private async Task<Job> GetLastCompletedFinishedTime(JobType jobType)
        {
            return await jobRepository.GetLastCompletedFinishedTime(jobType);
        }
    }
}
