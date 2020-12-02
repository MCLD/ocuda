using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class PodcastDirectoryInfo
    {
        [Key]
        [Required]
        public int PodcastId { get; set; }
        public Podcast Podcast { get; set; }

        [Key]
        [Required]
        public int PodcastDirectoryId { get; set; }
        public PodcastDirectory PodcastDirectory { get; set; }

        [MaxLength(255)]
        public string Url { get; set; }
    }
}
