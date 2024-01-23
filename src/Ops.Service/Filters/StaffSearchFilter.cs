namespace Ocuda.Ops.Service.Filters
{
    public class StaffSearchFilter : SearchFilter
    {
        public StaffSearchFilter()
        {
        }

        public StaffSearchFilter(int page) : base(page)
        {
        }

        public StaffSearchFilter(int page, int take) : base(page, take)
        {
        }

        public int AssociatedLocation { get; set; }
    }
}