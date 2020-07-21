﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Segment
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [DisplayName("Start Date")]
        public DateTime? StartDate { get; set; }

        [DisplayName("End Date")]
        public DateTime? EndDate { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        [NotMapped]
        public SegmentText SegmentText { get; set; }

        [NotMapped]
        public ICollection<string> SegmentLanguages { get; set; }
    }
}
