using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Service.Filters
{
    public class FeatureFilter : BaseFilter
    {
        public ICollection<int> FeatureIds { get; set; }
        public FeatureFilter(int? page = null, int take = 10) : base(page, take) { }
    }
}
