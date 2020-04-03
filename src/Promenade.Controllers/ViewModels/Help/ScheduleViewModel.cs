using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Helpers;

namespace Ocuda.Promenade.Controllers.ViewModels.Help
{
    public class ScheduleViewModel
    {
        public SegmentText SegmentText { get; set; }
        public IEnumerable<SelectListItem> Subjects { get; set; }
        public ScheduleRequest ScheduleRequest { get; set; }

        public SelectListItem[] Languages = new SelectListItem[]
        {
            new SelectListItem
            {
                 Text = "English",
                 Value = "English"
            },
            new SelectListItem
            {
                Text = "español",
                Value = "español"
            }
        };

        [Display(Name = "Requested date")]
        public System.DateTime RequestedDate { get; set; }

        [Display(Name = "Requested time")]
        public System.DateTime RequestedTime { get; set; }

        [Display(Name = "Subject")]
        public string ScheduleRequestSubject { get; set; }

        [Display(Name = "Phone")]
        [MaxLength(255)]
        [Required]
        public string ScheduleRequestPhone { get; set; }

        public string FormattedPhone
        {
            get
            {
                return TextFormattingHelper
                    .FormatPhone(ScheduleRequest?.ScheduleRequestTelephone?.Phone);
            }
        }
    }
}
