using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduleRequestSubject
    {
        public int? CancellationEmailSetupId { get; set; }

        public int? FollowupEmailSetupId { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        public int? RelatedEmailSetupId { get; set; }

        public bool RequireComments { get; set; }

        public bool RequireEmail { get; set; }

        public int? SegmentId { get; set; }

        [MaxLength(255)]
        [Required]
        public string Subject { get; set; }

        [NotMapped]
        public string SubjectText { get; set; }
    }
}