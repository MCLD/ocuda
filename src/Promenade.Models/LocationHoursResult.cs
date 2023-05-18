using System;

namespace Ocuda.Promenade.Models
{
    public class LocationHoursResult
    {
        public DateTime? CloseTime { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsCurrentlyOpen { get; set; }
        public bool IsOverride { get; set; }
        public bool IsSpecialHours { get; set; }
        public DateTime? NextOpenDateTime { get; set; }
        public DateTime? NextStatusChange { get; set; }
        public bool Open { get; set; }
        public DateTime? OpenTime { get; set; }
        public string StatusMessage { get; set; }
        public string TodaysHours { get; set; }
    }
}