using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Service.Filters
{
    public class GroupFilter : BaseFilter
    {
        public ICollection<int> GroupIds { get; set; }
        public GroupFilter(int? page = null, int take = 10) : base(page, take) { }
    }
}
