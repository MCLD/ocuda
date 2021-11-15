using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class ProductLocationInventory
    {
        public enum Status
        {
            None,
            Few,
            Many
        }

        public DateTime CreatedAt { get; set; }

        public int CreatedBy { get; set; }

        public Status InventoryStatus { get; set; }

        public Location Location { get; set; }

        [Key]
        [Required]
        public int LocationId { get; set; }

        public Product Product { get; set; }

        [Key]
        [Required]
        public int ProductId { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        [NotMapped]
        public string UpdatedByName { get; set; }

        [NotMapped]
        public string UpdatedByUsername { get; set; }
    }
}