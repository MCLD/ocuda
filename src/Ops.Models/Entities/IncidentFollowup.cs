using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class IncidentFollowup : Abstract.BaseEntity
    {
        [Required]
        [MaxLength(500)]
        public string Description { get; set; }

        public Incident Incident { get; set; }

        [Required]
        public int IncidentId { get; set; }
    }
}
