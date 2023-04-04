using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class UserSyncLocation : Abstract.BaseEntity
    {
        public int? MapToLocationId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
    }
}