using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Product
    {
        public DateTime CreatedAt { get; set; }

        public int CreatedBy { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        public bool IsActive { get; set; }

        public bool IsVisibleToPublic { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

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
