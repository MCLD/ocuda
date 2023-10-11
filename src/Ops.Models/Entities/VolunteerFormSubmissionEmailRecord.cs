using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class VolunteerFormSubmissionEmailRecord
    {
        public EmailRecord EmailRecord { get; set; }

        [Key]
        [Required]
        public int EmailRecordId { get; set; }

        public User User { get; set; }

        public int UserId { get; set; }

        [Key]
        [Required]
        public int VolunterFormSubmissionId { get; set; }
    }
}