using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class IncidentParticipant : Abstract.BaseEntity
    {
        [MaxLength(40)]
        public string Barcode { get; set; }

        [MaxLength(100)]
        public string Description { get; set; }

        public Incident Incident { get; set; }

        [Required]
        public int IncidentId { get; set; }

        [Required]
        public IncidentParticipantType IncidentParticipantType { get; set; }

        [MaxLength(50)]
        public string Name { get; set; }
    }
}