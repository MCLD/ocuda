using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class Page : Abstract.BaseEntity
    {
        public int SectionId { get; set; }
        public Section Section { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }

        [Required]
        [MaxLength(255)]
        public string Stub { get; set; }
        
        public string Content { get; set; }

        public DateTime? PublishedAt { get; set; }
    }
}
