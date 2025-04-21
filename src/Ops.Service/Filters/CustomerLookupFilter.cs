namespace Ocuda.Ops.Service.Filters
{
    public class CustomerLookupFilter : BaseFilter
    {
        public OrderType OrderBy { get; set; }
        public bool OrderDesc { get; set; }
        public string Search { get; set; }

        public CustomerLookupFilter(int? page = null) : base(page) { }

        public enum OrderType
        {
            NameFirst,
            NameLast,
            LastActivityDate
        };
    }
}
