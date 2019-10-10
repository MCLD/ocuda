using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Service.Filters
{
    public class PolarisPatronFilter : BaseFilter
    {
        public OrderType OrderBy { get; set; }

        public PolarisPatronFilter(int? page = null) : base(page) { }

        public enum OrderType
        {
            NameFirst,
            NameLast,
            LastActivityDate
        };

        public bool OnlyNoCheckouts { get; set; }
    }
}
