﻿using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models
{
    public class File : Abstract.BaseEntity
    {
        public int SectionId { get; set; }
        public Section Section { get; set; }
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        [Required]
        [MaxLength(255)]
        public string FilePath { get; set; }
        public string Icon { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }

        public bool IsFeatured { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
