using System.Collections.Generic;
using System.ComponentModel;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Podcasts
{
    public class EpisodePermissionsViewModel
    {
        [DisplayName("Podcast Title")]
        public string Title { get; set; }

        [DisplayName("Stub")]
        public string Stub { get; set; }

        public int PodcastId { get; set; }

        public IDictionary<int, string> AvailableGroups { get; set; }
        public IDictionary<int, string> AssignedGroups { get; set; }
    }
}