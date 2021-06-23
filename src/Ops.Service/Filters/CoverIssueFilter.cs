using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Filters
{
    public class CoverIssueFilter : BaseFilter
    {
        public CoverIssueType? CoverIssueType { get; set; }

        public CoverIssueFilter(int? page = null, int take = 15) : base(page, take) { }
    }
}
