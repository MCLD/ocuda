using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class Section : Abstract.BaseEntity
    {
        [MaxLength(255)]
        public string EmbedVideoUrl { get; set; }

        [MaxLength(255)]
        public string Icon { get; set; }

        public bool IsHomeSection { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        [Required]
        public string Slug { get; set; }

        public bool SupervisorsOnly { get; set; }

        [NotMapped]
        public int PostCount { get; set; }

        [NotMapped]
        public int FileLibraryCount { get; set; }

        [NotMapped]
        public int LinkLibraryCount { get; set; }
    }
}
