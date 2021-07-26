using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility;
using Ocuda.Utility.Helpers;

namespace Ocuda.Promenade.Controllers.ViewModels.Help
{
    public class ScheduleDetailsViewModel
    {
        public static IEnumerable<SelectListItem> Languages
        {
            get
            {
                return i18n.Culture.SupportedCultures
                    .Select(_ => new SelectListItem
                    {
                        Text = _.NativeName,
                        Value = _.Name,
                        Selected = _.Name == CultureInfo.CurrentCulture.Name
                    });
            }
        }

        [Display(Name = "Subject")]
        public string DisplaySubject { get; set; }

        [Display(Name = "Requested date and time")]
        public string DisplayTime
        {
            get
            {
                return ScheduleRequest.RequestedTime.ToString("f", CultureInfo.CurrentCulture);
            }
        }

        public string EmailRequiredMessage { get; set; }
        public bool ForceReload { get; set; }

        public string FormattedLanguage
        {
            get
            {
                return new CultureInfo(ScheduleRequest?.Language).NativeName;
            }
        }

        public string FormattedPhone
        {
            get
            {
                return TextFormattingHelper
                    .FormatPhone(ScheduleRequest?.ScheduleRequestTelephone?.Phone);
            }
        }

        public string NotesRequiredMessage { get; set; }
        public ScheduleRequest ScheduleRequest { get; set; }

        [Display(Name = "Phone")]
        [MaxLength(255)]
        [Required(ErrorMessage = ErrorMessage.FieldRequired)]
        public string ScheduleRequestPhone { get; set; }

        public SegmentText SegmentText { get; set; }
        public bool ShowEmailMessage { get; set; }
    }
}