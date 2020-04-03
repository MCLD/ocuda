using System;
using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Contact
{
    public class ScheduleIndexViewModel
    {
        public string ViewDescription { get; set; }
        public IEnumerable<ScheduleRequest> Requests { get; set; }
        public IEnumerable<ScheduleClaim> Claims { get; set; }
        public DateTime RequestedDate { get; set; }
        public string ActiveToday
        {
            get

            {
                return ViewDescription == System.DateTime.Now.ToShortDateString()
                    ? "active"
                    : null;
            }
        }

        public string ActiveUnclaimed
        {
            get
            {
                return ViewDescription == "Unclaimed"
                    ? "active"
                    : null;
            }
        }
    }
}
