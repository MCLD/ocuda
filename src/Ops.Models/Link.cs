using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Ocuda.Ops.Models
{
    public class Link : Abstract.BaseEntity
    {
        public int SectionId { get; set; }
        public Section Section { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [Required]
        [MaxLength(255)]
        public string Url { get; set; }

        public bool IsFeatured { get; set; }
    }
}
