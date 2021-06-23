namespace Ocuda.Promenade.Service.Filters
{
    public class PodcastFilter : BaseFilter
    {
        public bool SerialOrdering { get; set; }

        public PodcastFilter(int? page = null, int take = 10) : base(page, take) { }
    }
}
