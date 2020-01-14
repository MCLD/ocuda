using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationHours
    {
        [Key]
        [Required]
        public int LocationId { get; set; }

        public Location Location { get; set; }

        [Key]
        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        public bool Open { get; set; }
        public DateTime? OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }
    }
}
