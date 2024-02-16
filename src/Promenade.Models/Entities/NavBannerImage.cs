using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavBannerImage
    {
        [Key]
        [Required]
        public int NavBannerId { get; set; }
        public NavBanner NavBanner { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [Required]
        [MaxLength(255)]
        [DisplayName("Image alternative text")]
        [Description("How should this image be described to someone who can't see it?")]
        public string ImageAltText { get; set; }
    }
}
