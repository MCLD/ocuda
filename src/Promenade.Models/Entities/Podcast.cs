using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        public string ImageUrl { get; set; }

        [MaxLength(8)]
        [Required]
        public string Language { get; set; }

        public bool IsExplicit { get; set; }

        public bool IsSerial { get; set; }

        [MaxLength(255)]
        [Required]
        public string OwnerName { get; set; }

        [MaxLength(255)]
        [Required]
        public string OwnerEmail { get; set; }

        public bool IsBlocked { get; set; }

        public bool IsCompleted { get; set; }

        public bool IsDeleted { get; set; }

        public ICollection<PodcastItem> PodcastItems { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
