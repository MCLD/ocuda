using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Podcasts
{
    public class EpisodeViewModel
    {
        public Podcast Podcast { get; set; }
        public PodcastItem PodcastItem { get; set; }
    }
}
