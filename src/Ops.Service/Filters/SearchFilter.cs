namespace Ocuda.Ops.Service.Filters
{
    public class SearchFilter : BaseFilter
    {
        public SearchFilter() : base() { }

        public SearchFilter(int page) : base(page) { }

        public string SearchText { get; set; }
    }
}
