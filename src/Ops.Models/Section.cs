using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models
{
    public class Section : Abstract.BaseEntity
    {
        [MaxLength(64)]
        public string Path { get; set; }
        [MaxLength(255)]
        [Required]
        public string Name { get; set; }
        [MaxLength(32)]
        public string Icon { get; set; }
        public int SortOrder { get; set; }
        [DisplayName("Video URL")]
        public string FeaturedVideoUrl { get; set; }
        public bool IsDeleted { get; set; }
    }
}
