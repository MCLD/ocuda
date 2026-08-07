using System;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Models.Definitions.Models
{
    public class JobDefinition
    {
        /// <summary>
        /// Gets or sets a value indicating whether this job should automatically schedule by
        /// default.
        /// </summary>
        public bool DefaultAutomaticallySchedule { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this job is eligible to be run manually by a
        /// user.
        /// </summary>
        public bool DefaultCanRunManually { get; set; }

        /// <summary>
        /// Gets or sets a value for how many seconds must elapse between runs of this job.
        /// </summary>
        public int? DefaultMinimumSecondsBetweenRuns { get; set; }

        /// <summary>
        /// Gets or sets a description of this job.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of this job definition, should be unique.
        /// </summary>
        public JobType JobType { get; set; }

        /// <summary>
        /// Gets or sets the friendly name of this job.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the method which runs the job. The prototype for this method looks like:
        ///
        /// Task RunJobAsync(Job job, Func.<Job, Task> statusAsync);
        ///
        /// Where job is the object describing the job and statusAsync is a method which accepts
        /// the job object with updated PercentComplete and Status properties and records the
        /// updates in the database and reports them to the user (if a user triggered the job).
        /// </summary>
        public Func<Job, Func<Job, Task>, Task> RunAsync { get; set; }
    }
}
