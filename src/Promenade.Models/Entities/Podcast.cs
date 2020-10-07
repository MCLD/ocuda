using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Podcast
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Title { get; set; }

        [MaxLength(50)]
        [Required]
        public string Stub { get; set; }

        [MaxLength(255)]
        public string Author { get; set; }

        [MaxLength(1000)]
        [Required]
        public string Description { get; set; }

        [MaxLength(1000)]
        [Required]
        public string Category { get; set; }

        [MaxLength(255)]
        [Required]
        [DisplayName("Image URL")]
        public string ImageUrl { get; set; }

        [MaxLength(255)]
        [DisplayName("Image Thumbnail URL")]
        public string ImageThumbnailUrl { get; set; }

        [MaxLength(8)]
        [Required]
        public string Language { get; set; }

        [DisplayName("Is Explicit")]
        public bool IsExplicit { get; set; }

        [DisplayName("Is Serial")]
        public bool IsSerial { get; set; }

        [MaxLength(255)]
        [Required]
        [DisplayName("Owner Name")]
        public string OwnerName { get; set; }

        [MaxLength(255)]
        [Required]
        [DisplayName("Owner Email")]
        public string OwnerEmail { get; set; }

        [DisplayName("Is Blocked")]
        public bool IsBlocked { get; set; }

        [DisplayName("Is Completed")]
        public bool IsCompleted { get; set; }

        [DisplayName("Is Deleted")]
        public bool IsDeleted { get; set; }

        public ICollection<PodcastItem> PodcastItems { get; set; }

        [DisplayName("Last Update")]
        public DateTime UpdatedAt { get; set; }

        [MaxLength(255)]
        public string Subtitle { get; set; }

        [MaxLength(255)]
        public string Copyright { get; set; }

        [NotMapped]
        public IEnumerable<string> PermissionGroupIds { get; set; }

        [NotMapped]
        [DisplayName("Number of Episodes")]
        public int EpisodeCount { get; set; }
    }
}
