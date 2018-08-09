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
        [DisplayName("Sort Order")]
        public int SortOrder { get; set; }
        [DisplayName("Video URL")]
        public string FeaturedVideoUrl { get; set; }
        public bool IsDeleted { get; set; }

        [DisplayName("Show in Navigation Bar?")]
        public bool IsNavigation { get; set; }
    }
}
