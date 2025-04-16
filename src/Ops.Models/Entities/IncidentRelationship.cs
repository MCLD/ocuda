using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class IncidentRelationship
    {
        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedBy { get; set; }

        public User CreatedByUser { get; set; }

        [Key]
        [Required]
        public int IncidentId { get; set; }

        [Key]
        [Required]
        public int RelatedIncidentId { get; set; }
    }
}