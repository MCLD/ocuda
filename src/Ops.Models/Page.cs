using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models
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
        [Required]
        public string Content { get; set; }

        [DisplayName("Is this a Draft?")]
        public bool IsDraft { get; set; }
    }
}
