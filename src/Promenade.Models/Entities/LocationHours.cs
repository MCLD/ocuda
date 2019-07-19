using System;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationHours
    {
        public int Id { get; set; }

        public int LocationId { get; set; }
        public Location Location { get; set; }

        public DayOfWeek DayOfWeek { get; set; }
        public bool Open { get; set; }
        public DateTime? OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }
    }
}
