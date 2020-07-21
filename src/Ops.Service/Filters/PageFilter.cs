using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Filters
{
    public class PageFilter : BaseFilter
    {
        public PageType? PageType { get; set; }

        public PageFilter(int? page = null, int take = 15) : base(page, take) { }
    }
}
