using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class IncidentStaff : Abstract.BaseEntity
    {
        public Incident Incident { get; set; }

        [Required]
        public int IncidentId { get; set; }

        [Required]
        public IncidentParticipantType IncidentParticipantType { get; set; }

        public User User { get; set; }

        [Required]
        public int UserId { get; set; }
    }
}