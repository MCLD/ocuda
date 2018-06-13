using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models
{
    public class SectionPost : Abstract.BaseEntity
    {
        public int SectionId { get; set; }
        public Section Section { get; set; }
        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
        [Required]
        [MaxLength(255)]
        public string Stub { get; set; }
        [Required]
        [MaxLength(2000)]
        public string Content { get; set; }

        [DisplayName("Is this a Draft?")]
        public bool IsDraft { get; set; }

        [DisplayName("Pin this post?")]
        public bool IsPinned { get; set; }

        [DisplayName("Show on home page?")]
        public bool ShowOnHomepage { get; set; }
    }
}
