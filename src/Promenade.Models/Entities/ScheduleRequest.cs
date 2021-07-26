using System;
using System.ComponentModel.DataAnnotations;
using Ocuda.Utility;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduleRequest
    {
        public DateTime? CancellationSentAt { get; set; }

        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        public DateTime CreatedAt { get; set; }

        [MaxLength(255)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public DateTime? FollowupSentAt { get; set; }

        [Key]
        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        public int Id { get; set; }

        public bool IsCancelled { get; set; }

        public bool IsClaimed { get; set; }

        public bool IsUnderway { get; set; }

        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        [MaxLength(255)]
        [Display(Name = "Language")]
        public string Language { get; set; }

        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        [MaxLength(255)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [MaxLength(255)]
        [Display(Name = "How can we help?")]
        public string Notes { get; set; }

        public DateTime? NotificationSentAt { get; set; }

        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        [Display(Name = "Requested time")]
        public DateTime RequestedTime { get; set; }

        public ScheduleRequestSubject ScheduleRequestSubject { get; set; }

        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        [Display(Name = "Subject")]
        public int ScheduleRequestSubjectId { get; set; }

        public ScheduleRequestTelephone ScheduleRequestTelephone { get; set; }

        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        public int ScheduleRequestTelephoneId { get; set; }
    }
}