using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IJobRepository : IGenericRepository<Job>
    {
        /// <summary>
        /// Look up a single job record by job id.
        /// </summary>
        /// <param name="jobId">The <see cref="Job.Id"/> to look up.</param>
        /// <returns>A populated <see cref="Job"/> object from the database.</returns>
        public Task<Job> FindAsync(int jobId);

        /// <summary>
        /// Get the latest <see cref="Job.FinishedAt"/> time for a job of the specified
        /// <see cref="JobType"/>. This is used for scheduling the next occurance of the job.
        /// </summary>
        /// <param name="jobType">The <see cref="JobType"/> to look up.</param>
        /// <returns>The <see cref="Job"/> with the latest <see cref="Job.FinishedAt"/> time.
        /// </returns>
        public Task<Job> GetLastCompletedFinishedTime(JobType jobType);

        /// <summary>
        /// Get the most recently created job of the specified <see cref="JobType"/>. This is used
        /// to detect if this job hasn't run before or if it has an open task running.
        /// </summary>
        /// <param name="jobType">The <see cref="JobType"/> to look up.</param>
        /// <returns>The <see cref="Job"/> with the latest <see cref="Job.CreatedAt"/> time.
        /// </returns>
        public Task<Job> GetLastCreatedAsync(JobType jobType);

        /// <summary>
        /// Retreive jobs from the database that are ready to run.
        /// </summary>
        /// <returns>An <see cref="IEnumerable"/> of the jobs that are ready to run.</returns>
        public Task<IEnumerable<Job>> GetPendingAsync();
    }
}
