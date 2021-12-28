using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Podcasts
{
    public class ShowNotesViewModel
    {
        public Podcast Podcast { get; set; }
        public PodcastItem PodcastItem { get; set; }
        public SegmentText ShowNotesSegment { get; set; }
    }
}
