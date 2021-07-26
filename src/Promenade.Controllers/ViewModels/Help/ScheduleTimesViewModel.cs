using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Help
{
    public class ScheduleTimesViewModel
    {
        [Display(Name = "Requested date and time")]
        public string DisplayTime
        {
            get
            {
                return ScheduleRequestTime.ToString("f", CultureInfo.CurrentCulture);
            }
        }

        [Display(Name = "Requested date")]
        public DateTime? RequestedDate { get; set; }

        [Display(Name = "Requested time")]
        public DateTime? RequestedTime { get; set; }

        public DateTime ScheduleRequestTime { get; set; }
        public SegmentText SegmentText { get; set; }

        public DateTime? SelectedTime { get; set; }
        public ICollection<DateTime> SuggestedTimes { get; set; }
        public IEnumerable<SelectListItem> TimeBlocks { get; set; }
    }
}