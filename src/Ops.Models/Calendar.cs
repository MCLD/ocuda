using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocuda.Ops.Models
{
    public class Calendar : Abstract.BaseEntity
    {
        public int SectionId { get; set; }
        public Section Section { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [Required]
        public DateTime When { get; set; }
        [MaxLength(255)]
        public string Url { get; set; }

        public bool IsDraft { get; set; }
        public bool IsPinned { get; set; }
        public bool ShowOnHomepage { get; set; }
    }
}
