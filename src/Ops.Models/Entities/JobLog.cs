using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    /// <summary>
    /// This is log output from the run of a job.
    /// </summary>
    public class JobLog : Abstract.BaseEntity<int>
    {
        [Required]
        public int JobId { get; set; }

        public int? PercentComplete { get; set; }

        [Required]
        [MaxLength(255)]
        public string Status { get; set; }
    }
}
