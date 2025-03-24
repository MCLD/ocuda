namespace Ocuda.Ops.Controllers.Areas.BooksByMail.QueryFilters
{
    public class PolarisPatronFilter : BaseFilter
    {
        public OrderType OrderBy { get; set; }

        public PolarisPatronFilter(int? page = null) : base(page) { }

        public enum OrderType
        {
            NameFirst,
            NameLast,
            LastActivityDate
        };
    }
}
