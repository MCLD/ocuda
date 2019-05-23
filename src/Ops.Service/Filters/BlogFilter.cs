using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Filters
{
    public class BlogFilter : BaseFilter
    {
        public int? SectionId { get; set; }
        public int? PostCategoryId { get; set; }
        public int? FileLibraryId { get; set; }
        public int? LinkLibraryId { get; set; }

        public BlogFilter(int? page = null, int take = 15) : base(page, take) { }
    }
}
