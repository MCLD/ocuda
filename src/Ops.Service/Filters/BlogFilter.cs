using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Filters
{
    public class BlogFilter : BaseFilter
    {
        public int? CategoryId { get; set; }
        public int? SectionId { get; set; }
        public CategoryType? CategoryType { get; set; }

        public BlogFilter(int? page = null, int? take = null) : base(page, take = 15) { }
    }
}
