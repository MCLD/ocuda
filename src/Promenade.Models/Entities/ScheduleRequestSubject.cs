using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduleRequestSubject
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Subject { get; set; }

        public int? SegmentId { get; set; }

        public bool RequireEmail { get; set; }
        public bool RequireComments { get; set; }
        public int? RelatedEmailSetupId { get; set; }
    }
}
