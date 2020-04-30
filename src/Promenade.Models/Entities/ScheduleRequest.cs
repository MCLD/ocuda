using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduleRequest
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Requested time")]
        public DateTime RequestedTime { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public int ScheduleRequestSubjectId { get; set; }
        public ScheduleRequestSubject ScheduleRequestSubject { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        public int ScheduleRequestTelephoneId { get; set; }
        public ScheduleRequestTelephone ScheduleRequestTelephone { get; set; }

        [Required]
        [MaxLength(255)]
        public string Language { get; set; }

        [MaxLength(255)]
        [Display(Name = "How can we help?")]
        public string Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public bool IsClaimed { get; set; }
    }
}
