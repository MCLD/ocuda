using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Helpers;

namespace Ocuda.Ops.Controllers.ViewModels.Contact
{
    public class ScheduleDetailViewModel
    {
        public ScheduleDetailViewModel()
        {
            FinishMessage = "Yes";
        }

        public ScheduleRequest ScheduleRequest { get; set; }
        public ScheduleClaim ScheduleClaim { get; set; }
        public IEnumerable<ScheduleLog> ScheduleLogs { get; set; }

        public string FormattedPhone
        {
            get
            {
                return TextFormattingHelper
                    .FormatPhone(ScheduleRequest?.ScheduleRequestTelephone?.Phone);
            }
        }

        public bool IsClaimedByCurrentUser { get; set; }
        public ScheduleLog AddLog { get; set; }
        public IEnumerable<SelectListItem> CallDispositions { get; set; }
        public string FinishMessage { get; set; }
        public string RequestedDate { get; set; }
    }
}