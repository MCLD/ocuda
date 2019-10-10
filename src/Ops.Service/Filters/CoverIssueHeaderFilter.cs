using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Service.Filters
{
    public class CoverIssueHeaderFilter : BaseFilter
    {
        public OrderType OrderBy { get; set; }

        public CoverIssueHeaderFilter(int? page = null, int take = 10) : base(page, take) { }

        public enum OrderType
        {
            BibID,
            HasIssue,
            CreatedBy,
            CreatedAt,
            LastResolved
        };
    }
}
