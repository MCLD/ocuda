using Microsoft.AspNetCore.Http;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Podcasts
{
    public class EpisodeDetailsViewModel
    {
        public bool EditEpisode { get; set; }
        public string PodcastTitle { get; set; }
        public PodcastItem Episode { get; set; }

        [System.ComponentModel.DisplayName("Upload podcast file")]
        public IFormFile UploadedFile { get; set; }

        public System.DateTime? UploadedAt { get; set; }
        public string Filename { get; set; }
        public bool FileMissing { get; set; }
        public string MaximumFileSizeMB { get; set; }
        public string ShowNotesSegmentName { get; set; }
        public bool CanEditShowNotes { get; set; }
    }
}