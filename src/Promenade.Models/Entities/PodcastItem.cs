using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class PodcastItem
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int PodcastId { get; set; }
        public Podcast Podcast { get; set; }

        [MaxLength(255)]
        public string Title { get; set; }

        [MaxLength(255)]
        public string Subtitle { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string Link { get; set; }

        [MaxLength(255)]
        public string EnclosureUrl { get; set; }

        [MaxLength(255)]
        public string EnclosureType { get; set; }

        public int EnclosureLength { get; set; }

        public DateTime? PublishDate { get; set; }

        [MaxLength(255)]
        public string Author { get; set; }

        public int Duration { get; set; }

        public bool Explicit { get; set; }

        [MaxLength(255)]
        public string Guid { get; set; }
    }
}
