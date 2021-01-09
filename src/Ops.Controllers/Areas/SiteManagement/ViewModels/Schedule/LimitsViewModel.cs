using System;
using System.Collections.Generic;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Schedule
{
    public class LimitsViewModel
    {
        public IEnumerable<DayOfWeek> AvailableDays { get; set; }
        public DayOfWeek SelectedDay { get; set; }
        public Dictionary<int, int?> DayLimits { get; set; }
    }
}
