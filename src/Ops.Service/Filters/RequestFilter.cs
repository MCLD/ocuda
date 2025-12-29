namespace Ocuda.Ops.Service.Filters
{
    public class RequestFilter : BaseFilter
    {
        public RequestFilter(int page) : base(page)
        {
        }

        public bool? IsProcessed { get; set; }
    }
}
