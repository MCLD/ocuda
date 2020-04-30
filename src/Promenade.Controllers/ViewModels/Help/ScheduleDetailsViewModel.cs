using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Helpers;

namespace Ocuda.Promenade.Controllers.ViewModels.Help
{
    public class ScheduleDetailsViewModel
    {
        public SegmentText SegmentText { get; set; }

        public string EmailRequiredMessage { get; set; }
        public string NotesRequiredMessage { get; set; }

        [Display(Name = "Requested date and time")]
        public string DisplayTime
        {
            get
            {
                return ScheduleRequest.RequestedTime.ToShortDateString()
                    + " at "
                    + ScheduleRequest.RequestedTime.ToShortTimeString();
            }
        }

        [Display(Name = "Subject")]
        public string DisplaySubject { get; set; }

        public bool ForceReload { get; set; }

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
