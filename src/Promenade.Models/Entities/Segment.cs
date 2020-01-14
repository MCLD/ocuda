using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Segment
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public SegmentText SegmentText { get; set; }

        [NotMapped]
        public ICollection<string> SegmentLanguages { get; set; }
    }
}
