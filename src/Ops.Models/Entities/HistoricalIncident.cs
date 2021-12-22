using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Ocuda.Ops.Models.Entities
{
    public class HistoricalIncident : Abstract.BaseEntity
    {
        [MaxLength(3000)]
        public string ActionTaken { get; set; }

        [MaxLength(50)]
        [Required]
        public string Branch { get; set; }

        [MaxLength(2000)]
        public string Comments { get; set; }

        [Required]
        public DateTime DateReported { get; set; }

        [MaxLength(28)]
        [Required]
        public string DateReportedString { get; set; }

        [MaxLength(50)]
        public string DescribedBy { get; set; }

        [MaxLength(5000)]
        [Required]
        [Column(TypeName = "varchar(5000)")]
        public string Description { get; set; }

        [Required]
        public string Filename { get; set; }

        [Required]
        public DateTime IncidentAt { get; set; }

        [MaxLength(28)]
        [Required]
        public string IncidentAtString { get; set; }

        [MaxLength(30)]
        [Required]
        public string IncidentType { get; set; }

        [MaxLength(1500)]
        [Required]
        public string Location { get; set; }

        [MaxLength(1000)]
        public string PeopleInvolved { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; }

        [MaxLength(50)]
        [Required]
        public string ReportedBy { get; set; }

        [MaxLength(50)]
        public string ReportedByTitle { get; set; }

        [MaxLength(1500)]
        public string Witnesses { get; set; }
    }
}
