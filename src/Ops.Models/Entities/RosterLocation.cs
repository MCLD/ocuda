using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Interfaces;

namespace Ocuda.Ops.Models.Entities
{
    public class RosterLocation : Abstract.BaseEntity, IRosterLocationMapping
    {
        [Required]
        public int IdInRoster { get; set; }

        public int? MapToLocationId { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }
    }
}