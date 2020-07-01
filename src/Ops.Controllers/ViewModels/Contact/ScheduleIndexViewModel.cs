using System;
using System.Collections.Generic;
using System.Linq;
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
                return ViewDescription == DateTime.Now.ToShortDateString()
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

        public string GetRowClass(ScheduleRequest request)
        {
            if (request == null)
            {
                return null;
            }

            var claim = Claims.SingleOrDefault(_ => _.ScheduleRequestId == request.Id);

            if (claim?.IsComplete == true)
            {
                return "table-success";
            }

            if (request.RequestedTime.AddHours(1) < DateTime.Now
                && claim?.IsComplete != true
                && request.IsUnderway)
            {
                return "table-warning";
            }

            if (request?.IsClaimed == true)
            {
                return "table-info";
            }

            if (request.RequestedTime.AddHours(-1) < DateTime.Now
                && request?.IsClaimed != true
                && !request.IsUnderway)
            {
                return "table-danger";
            }

            return null;
        }

        public string GetStatusTag(ScheduleRequest request)
        {
            if (request == null)
            {
                return null;
            }

            var claim = Claims.SingleOrDefault(_ => _.ScheduleRequestId == request.Id);

            if (claim?.IsComplete == true)
            {
                return "<span class=\"far fa-check-square mr-1\" title=\"Complete\"></span>";
            }

            if (request.RequestedTime.AddHours(1) < DateTime.Now
                && request?.IsClaimed == true
                && request.IsUnderway)
            {
                return "<span class=\"fas fa-tasks mr-1\" title=\"Underway, not yet complete\"></span>";
            }

            if (request?.IsClaimed == true)
            {
                return request.IsUnderway
                    ? "<span class=\"fas fa-user mr-1\" title=\"Claimed, underway\"></span>"
                    : "<span class=\"far fa-user mr-1\" title=\"Claimed, not started\"></span>";
            }


            if (request.RequestedTime.AddHours(1) < DateTime.Now
                && request?.IsClaimed != true
                && !request.IsUnderway)
            {
                return "<span class=\"fas fa-exclamation-triangle mr-1\" title=\"Unclaimed, overdue\"></span>";
            }

            if (request.RequestedTime.AddHours(-1) < DateTime.Now
                && request?.IsClaimed != true
                && !request.IsUnderway)
            {
                return "<span class=\"fas fa-clock mr-1\" title=\"Unclaimed, scheduled soon\"></span>";
            }

            return null;
        }
    }
}
