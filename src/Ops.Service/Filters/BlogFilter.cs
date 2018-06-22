using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Service.Filters
{
    public class BlogFilter : BaseFilter
    {
        public int? CategoryId { get; set; }
        public int? SectionId { get; set; }


        public BlogFilter(int? page = null, int? take = null) : base(page, take) { }
    }
}
