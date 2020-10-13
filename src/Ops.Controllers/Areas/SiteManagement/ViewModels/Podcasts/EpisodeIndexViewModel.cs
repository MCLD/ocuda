using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Podcasts
{
    public class EpisodeIndexViewModel
    {
        public int PodcastId { get; set; }
        public string PodcastTitle { get; set; }
        public ICollection<PodcastItem> Episodes { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public bool IsSiteManager { get; set; }
        public bool HasPermission { get; set; }
    }
}
