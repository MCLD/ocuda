using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Product
    {
        [Required]
        [Display(Name = "Minutes to cache inventory lookups")]
        public int CacheInventoryMinutes { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CreatedBy { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [Display(Name = "Is it active?")]
        public bool IsActive { get; set; }

        [Display(Name = "Visible to the public?")]
        public bool IsVisibleToPublic { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Display(Name = "Segment text")]
        public int? SegmentId { get; set; }

        [NotMapped]
        public SegmentText SegmentText { get; set; }

        [Required]
        [MaxLength(255)]
        public string Slug { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }
    }
}
