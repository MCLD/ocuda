namespace Ocuda.Ops.Service.Filters
{
    public class BaseFilter
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }

        public BaseFilter(int? page = null, int take = 15)
        {
            Skip = page.HasValue ? Take * (page - 1) : 0;
            Take = take;
        }
    }
}
