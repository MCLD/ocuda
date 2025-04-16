using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class ScheduleLog
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Display(Name = "Call disposition")]
        public int? ScheduleLogCallDispositionId { get; set; }

        public ScheduleLogCallDisposition ScheduleLogCallDisposition { get; set; }

        [Display(Name = "Duration (minutes)")]
        public int? DurationMinutes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [MaxLength(1000)]
        public string Notes { get; set; }

        [Required]
        public int ScheduleRequestId { get; set; }

        public int UserId { get; set; }

        [Display(Name = "Call is complete")]
        public bool IsComplete { get; set; }

        [NotMapped]
        public bool ShowNotesHeader
        {
            get
            {
                return IsComplete
                    || !string.IsNullOrEmpty(ScheduleLogCallDisposition?.Disposition)
                    || DurationMinutes != null;
            }
        }

        [NotMapped]
        public string Name { get; set; }

        [NotMapped]
        public string Username { get; set; }

        public int? RelatedEmailId { get; set; }
        public bool IsCancelled { get; set; }
    }
}