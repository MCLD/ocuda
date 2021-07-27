namespace Ocuda.Ops.Service.Filters
{
    public class BlogFilter : BaseFilter
    {
        public BlogFilter(int? page = null, int take = 15) : base(page, take)
        {
        }

        public int? CategoryId { get; set; }
        public int? FileLibraryId { get; set; }
        public bool? IsShownOnHomePage { get; set; }
        public int? LinkLibraryId { get; set; }
        public int? SectionId { get; set; }
    }
}