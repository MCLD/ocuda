namespace Ocuda.Ops.Controllers.Areas.BooksByMail.QueryFilters
{
    public class PolarisItemFilter : BaseFilter
    {
        public int PatronID { get; set; }
        public OrderType OrderBy { get; set; }

        public PolarisItemFilter(int? page = null, int take = 10) : base(page, take) { }

        public enum OrderType
        {
            Title,
            Author,
            CheckoutDate
        };
    }
}
