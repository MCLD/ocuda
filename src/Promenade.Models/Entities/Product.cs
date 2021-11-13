using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class Product
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Slug { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }
    }
}
