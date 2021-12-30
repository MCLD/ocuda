using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class IncidentType : Abstract.BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Description { get; set; }
    }
}
