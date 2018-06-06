using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models
{
    public class SectionFile : Abstract.BaseEntity
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

        public bool IsDraft { get; set; }
        public bool IsPinned { get; set; }
        public bool ShowOnHomepage { get; set; }
    }
}
