using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ScheduleRequestLimit
    {
        [Key]
        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Key]
        [Required]
        public int Hour { get; set; }

        [Range(0, int.MaxValue)]
        public int Limit { get; set; }
    }
}
