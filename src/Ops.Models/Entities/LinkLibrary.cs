using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class LinkLibrary : Abstract.BaseEntity
    {
        public LinkLibrary()
        {
            Links = [];
        }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public bool IsFeatured { get; set; }

        public bool IsNavigation { get; set; }

        public ICollection<Link> Links { get; }

        [MaxLength(255)]
        public string Slug { get; set; }

        public int SectionId { get; set; }

        [NotMapped]
        public int TotalLinksInLibrary { get; set; }
    }
}
