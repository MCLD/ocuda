using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduleRequestSubjectText
    {
        [Required]
        public int LanguageId { get; set; }

        [Required]
        public int ScheduleRequestSubjectId { get; set; }

        [Required]
        [MaxLength(255)]
        public string SubjectText { get; set; }
    }
}