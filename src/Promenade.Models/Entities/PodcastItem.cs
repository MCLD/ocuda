using System;
using System.ComponentModel;
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
        [Required]
        public string Title { get; set; }

        [MaxLength(50)]
        [Required]
        public string Stub { get; set; }

        [MaxLength(1000)]
        [Required]
        public string Description { get; set; }

        public bool IsExplicit { get; set; }

        [MaxLength(255)]
        public string ImageUrl { get; set; }

        [MaxLength(255)]
        public string Keywords { get; set; }

        public int? Season { get; set; }
        public int? Episode { get; set; }

        [MaxLength(255)]
        [Required]
        public string MediaUrl { get; set; }

        [MaxLength(32)]
        [Required]
        public string MediaType { get; set; }

        public int MediaSize { get; set; }

        public int Duration { get; set; }

        [MaxLength(255)]
        [Required]
        public string Guid { get; set; }

        public bool GuidPermaLink { get; set; }

        [DisplayName("Publish date")]
        public DateTime? PublishDate { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime UpdatedAt { get; set; }

        [MaxLength(255)]
        public string Subtitle { get; set; }

        [DisplayName("Show Notes Segment")]
        public int? ShowNotesSegmentId { get; set; }
    }
}
