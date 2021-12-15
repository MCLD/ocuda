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

        [NotMapped]
        public Status InventoryStatus
        {
            get
            {
                return GetStatus(ItemCount);
            }
        }

        public int? ItemCount { get; set; }

        public Location Location { get; set; }

        [Key]
        [Required]
        public int LocationId { get; set; }

        public int ManyThreshhold { get; set; }

        public Product Product { get; set; }

        [Key]
        [Required]
        public int ProductId { get; set; }

        public DateTime? ThreshholdUpdatedAt { get; set; }

        public int? ThreshholdUpdatedBy { get; set; }

        [NotMapped]
        public string ThreshholdUpdatedByName { get; set; }

        [NotMapped]
        public string ThreshholdUpdatedByUsername { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        [NotMapped]
        public string UpdatedByName { get; set; }

        [NotMapped]
        public string UpdatedByUsername { get; set; }

        public Status GetStatus(int? itemCount)
        {
            if (!itemCount.HasValue || itemCount == 0)
            {
                return Status.None;
            }
            if (itemCount >= ManyThreshhold)
            {
                return Status.Many;
            }
            return Status.Few;
        }
    }
}