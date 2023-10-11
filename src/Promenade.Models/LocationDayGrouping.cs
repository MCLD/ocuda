using System;
using System.Collections.Generic;

namespace Ocuda.Promenade.Models
{
    public class LocationDayGrouping
    {
        public LocationDayGrouping()
        {
            DaysOfWeek = new List<DayOfWeek>();
        }

        public TimeSpan Close { get; set; }
        public string Days { get; set; }
        public ICollection<DayOfWeek> DaysOfWeek { get; }
        public TimeSpan Open { get; set; }
        public string Time { get; set; }
    }
}