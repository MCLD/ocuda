namespace Ocuda.Ops.Service.Filters
{
    public class EmployeeCardFilter : BaseFilter
    {
        public EmployeeCardFilter(int page) : base(page)
        {
        }

        public bool? IsProcessed { get; set; }
    }
}
