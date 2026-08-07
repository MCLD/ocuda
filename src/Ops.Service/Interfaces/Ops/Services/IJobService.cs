using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IJobService
    {
        public Task AddJobLogAsync(JobLog jobLog);

        /// <summary>
        /// Ensure all job definitions have corresponding job configurations in the database.
        /// Typically run at application startup.
        /// </summary>
        /// <param name="userId">The user to insert job configurations as - typically the automated
        /// admin user.</param>
        /// <returns>Task returns when async operation is complete.</returns>
        public Task EnsureJobConfigurationsAsync(int userId);

        /// <summary>
        /// Get a <see cref="JobDefinition"/> object for the provided <see cref="JobType"/>.
        /// </summary>
        /// <param name="jobType">The <see cref="JobType"/> item we are looking up.</param>
        /// <returns>A populated <see cref="JobDefinition"/> from the database.</returns>
        public JobDefinition GetDefinition(JobType jobType);

        /// <summary>
        /// Retreive jobs from the database that are ready to run.
        /// </summary>
        /// <returns>An <see cref="IEnumerable"/> of the jobs that are ready to run.</returns>
        public Task<IEnumerable<Job>> GetPendingJobsAsync();

        /// <summary>
        /// Perform the logic to schedule all <see cref="Job"/> objects that have a
        /// <see cref="JobConfiguration>"/> indicating they can be automatically scheduled. Honors
        /// the specified <see cref="JobConfiguration.MinimumSecondsBetweenRuns"/>.
        /// </summary>
        /// <param name="userId">The user to schedule jobs as - typically the automated admin user.
        /// </param>
        /// <returns>Task returns when async operation is complete.</returns>
        public Task ScheduleJobsAsync(int userId);

        /// <summary>
        /// Update the job record in the database with information about its execution progress.
        /// </summary>
        /// <param name="job">The <see cref="Job"/> object describing the job to be run.</param>
        /// <returns>Task returns when async operation is complete.</returns>
        public Task UpdateJobAsync(Job job);
    }
}
