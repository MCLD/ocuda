using System.ComponentModel.DataAnnotations;

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
        public string Stub { get; set; }

        public bool SupervisorsOnly { get; set; }
    }
}