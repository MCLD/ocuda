namespace Ocuda.Ops.Service.Filters
{
    public class PolarisItemFilter : BaseFilter
    {
        public int PatronID { get; set; }
        public OrderType OrderBy { get; set; }
        public bool OrderDesc { get; set; }
        public string Search { get; set; }

        public PolarisItemFilter(int? page = null, int take = 10) : base(page, take) { }

        public enum OrderType
        {
            Title,
            Author,
            CheckoutDate
        };
    }
}
