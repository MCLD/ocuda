using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class Incident : Abstract.BaseEntity
    {
        [MaxLength(2500)]
        [Display(Name = "Describe the incident")]
        [Required]
        public string Description { get; set; }

        [Display(Name = "Follow-up information")]
        public ICollection<IncidentFollowup> Followups { get; set; }

        [Required]
        [Display(Name = "Date & time of incident")]
        public DateTime IncidentAt { get; set; }

        public IncidentType IncidentType { get; set; }

        [Display(Name = "Type of incident")]
        [Required]
        public int IncidentTypeId { get; set; }

        [MaxLength(1000)]
        [Required]
        [Display(Name = "Information about injuries or description of property damaged")]
        public string InjuriesDamages { get; set; }

        [Required]
        public bool IsVisible { get; set; }

        [Required]
        [Display(Name = "Law enforcement was contacted")]
        public bool LawEnforcementContacted { get; set; }

        [MaxLength(400)]
        [Required]
        [Display(Name = "Describe where the incident occurred")]
        public string LocationDescription { get; set; }

        [Required]
        [Display(Name = "Select where the incident occurred")]
        public int LocationId { get; set; }

        [NotMapped]
        public string LocationName { get; set; }

        public ICollection<IncidentParticipant> Participants { get; set; }

        [NotMapped]
        public DateTime RelatedAt { get; set; }

        [NotMapped]
        public User RelatedByUser { get; set; }

        [NotMapped]
        public ICollection<Incident> RelatedIncidents { get; set; }

        [Display(Name = "Your name")]
        [MaxLength(50)]
        public string ReportedByName { get; set; }

        public ICollection<IncidentStaff> Staffs { get; set; }
    }
}