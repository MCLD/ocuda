using System.Collections.Generic;

namespace Ocuda.Ops.Service.Filters
{
    public class IncidentFilter : SearchFilter
    {
        public IncidentFilter() : base()
        {
        }

        public IncidentFilter(int page) : base(page)
        {
        }

        public int? CreatedById { get; set; }

        public IEnumerable<int> IncludeIds { get; set; }
        public IEnumerable<int> LocationIds { get; set; }
    }
}
