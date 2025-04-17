namespace Ocuda.Ops.Service.Filters
{
    public class MaterialFilter : BaseFilter
    {
        public int CustomerLookupID { get; set; }
        public OrderType OrderBy { get; set; }
        public bool OrderDesc { get; set; }
        public string Search { get; set; }

        public MaterialFilter(int? page = null, int take = 10) : base(page, take)
        {
        }

        public enum OrderType
        {
            Title,
            Author,
            CheckoutDate
        };
    }
}