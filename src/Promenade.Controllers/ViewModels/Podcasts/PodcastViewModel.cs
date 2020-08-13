using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Controllers.ViewModels.Podcasts
{
    public class PodcastViewModel
    {
        public Podcast Podcast { get; set; }
        public ICollection<PodcastDirectoryInfo> PodcastDirectoryInfos { get; set; }
        public ICollection<PodcastItem> PodcastItems { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public bool ShowEpisodeImages { get; set; }
    }
}
