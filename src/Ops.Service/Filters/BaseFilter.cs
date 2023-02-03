namespace Ocuda.Ops.Service.Filters
{
    public class BaseFilter
    {
        private readonly int? _page;

        public BaseFilter(int? page = null, int take = 15)
        {
            _page = page;
            Take = take;
            Skip = _page.HasValue ? Take * (_page - 1) : 0;
        }

        public int Page
        { get { return _page ?? 1; } }

        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}