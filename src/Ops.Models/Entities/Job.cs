using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using Ocuda.Ops.Models.Definitions;

namespace Ocuda.Ops.Models.Entities
{
    /// <summary>
    /// This is a job that is either scheduled, underway, or completed.
    /// </summary>
    public class Job : Abstract.BaseEntity<int>
    {
        /// <summary>
        /// Gets or sets an optional CancellationToken for ending the job if it is supported.
        /// </summary>
        [NotMapped]
        public CancellationToken? CancellationToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if the job was cancelled during the run.
        /// </summary>
        public DateTime? CancelledAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the end time. Unset indicates it hasn't finished yet.
        /// </summary>
        public DateTime? FinishedAt { get; set; }

        /// <summary>
        /// Gets or sets the type of job this queue entry references.
        /// </summary>
        [Required]
        public JobType JobType { get; set; }

        /// <summary>
        /// Gets or sets an optional value indicating the percent complete for the job task.
        /// </summary>
        [NotMapped]
        public int? PercentComplete { get; set; }

        /// <summary>
        /// Gets or sets the IProgress used to reflect status to the user if triggered.
        /// </summary>
        [NotMapped]
        public IProgress<Job> Progress { get; set; }

        /// <summary>
        /// Gets or sets any optional parameters necessary to run the job.
        /// </summary>
        public string SerializedParameters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the start time. Unset indicates it hasn't run yet.
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the status of the job. UpdatedAt and UpdatedBy should
        /// pertain to this information as well.
        /// </summary>
        [NotMapped]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the user id who kicked the job off, admin if scheduled, a user if triggered.
        /// </summary>
        [NotMapped]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the job was successfully completed.
        /// </summary>
        public bool WasSuccessful { get; set; }
    }
}
