namespace BooksByMail.QueryFilters
{
    public class BaseFilter
    {
        public bool OrderDesc { get; set; }
        public string Search { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }

        public BaseFilter(int? page = null, int take = 15)
        {
            Take = take;
            Skip = page.HasValue ? Take * (page - 1) : 0;
        }
    }
}
