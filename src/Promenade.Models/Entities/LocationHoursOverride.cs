using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationHoursOverride
    {
        public int Id { get; set; }

        public int? LocationId { get; set; }
        public Location Location { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; }

        public DateTime Date { get; set; }
        public bool Open { get; set; }
        public DateTime? OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }
    }
}
