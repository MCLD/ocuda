namespace Ocuda.Ops.Service.Filters
{
    public class SearchFilter : BaseFilter
    {
        public SearchFilter()
        {
        }

        public SearchFilter(int page) : base(page)
        {
        }

        public SearchFilter(int page, int take) : base(page, take)
        {
        }

        public string SearchText { get; set; }
    }
}