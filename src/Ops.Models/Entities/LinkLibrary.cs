using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class LinkLibrary : Abstract.BaseEntity
    {
        public int SectionId { get; set; }
        public Section Section { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public bool IsNavigation { get; set; }

        public ICollection<Link> Links { get; set; }
    }
}
