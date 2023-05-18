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

        public string Days { get; set; }
        public ICollection<DayOfWeek> DaysOfWeek { get; }
        public string Time { get; set; }
    }
}