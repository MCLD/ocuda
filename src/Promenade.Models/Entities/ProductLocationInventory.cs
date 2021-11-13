using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class ProductLocationInventory
    {
        [Key]
        [Required]
        public int ProductId { get; set; }

        public Product Product { get; set; }

        [Key]
        [Required]
        public int LocationId { get; set; }
        
        public Location Location { get; set; }

        public Status InventoryStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        [NotMapped]
        public string UpdatedByName { get; set; }

        [NotMapped]
        public string UpdatedByUsername { get; set; }

        public enum Status
        {
            None,
            Low,
            High
        }
    }
}
