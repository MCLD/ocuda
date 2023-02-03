namespace Ocuda.Ops.Service.Filters
{
    public class LocationFilter : BaseFilter
    {
        public LocationFilter(int page, int take) : base(page, take)
        {
        }

        public LocationFilter(int page) : base(page)
        {
        }

        public bool IsDeleted { get; set; }
    }
}