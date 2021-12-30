using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class Incident : Abstract.BaseEntity
    {
        [MaxLength(2500)]
        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime IncidentAt { get; set; }

        public IncidentType IncidentType { get; set; }

        [Required]
        public int IncidentTypeId { get; set; }

        [MaxLength(1000)]
        [Required]
        public string InjuriesDamages { get; set; }

        [Required]
        public bool IsVisible { get; set; }

        [Required]
        public bool LawEnforcementContacted { get; set; }

        [MaxLength(400)]
        [Required]
        public string LocationDescription { get; set; }

        [Required]
        public int LocationId { get; set; }
    }
}
