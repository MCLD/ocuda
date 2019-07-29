using System;

namespace Ocuda.Promenade.Models
{
    public class LocationHoursResult
    {
        public bool Open { get; set; }
        public DateTime? OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }
        public DayOfWeek DayOfWeek { get; set; }

        public bool IsCurrentlyOpen { get; set; }
        public bool IsOverride { get; set; }
        public string StatusMessage { get; set; }
    }
}
