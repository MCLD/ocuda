using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Definitions;

namespace Ocuda.Ops.Models.Entities
{
    /// <summary>
    /// This class supports settings for particular job types. It is keyed off of JobType.
    /// </summary>
    public class JobConfiguration : Abstract.BaseEntity<JobType>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this job should be automatically scheduled to
        /// run.
        /// </summary>
        [Required]
        public bool AutomaticallySchedule { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if a user can manually run this job.
        /// </summary>
        [Required]
        public bool CanRunManually { get; set; }

        /// <summary>
        /// Gets or sets optional setting for how long to required to elapse before automated job
        /// runs. Null means no limit.
        /// </summary>
        public int? MinimumSecondsBetweenRuns { get; set; }
    }
}
