namespace Ocuda.Ops.Service.Filters
{
    public class PolarisPatronFilter : BaseFilter
    {
        public OrderType OrderBy { get; set; }
        public bool OrderDesc { get; set; }
        public string Search { get; set; }

        public PolarisPatronFilter(int? page = null) : base(page) { }

        public enum OrderType
        {
            NameFirst,
            NameLast,
            LastActivityDate
        };
    }
}
