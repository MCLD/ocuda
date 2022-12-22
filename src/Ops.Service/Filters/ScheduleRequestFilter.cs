using System;

namespace Ocuda.Ops.Service.Filters
{
    public class ScheduleRequestFilter : BaseFilter
    {
        public ScheduleRequestFilter(int page) : base(page)
        {
        }

        public bool IsCancelled { get; set; }
        public DateTime? RequestedDate { get; set; }
    }
}