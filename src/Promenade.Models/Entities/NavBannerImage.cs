using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavBannerImage
    {
        [MaxLength(255)]
        public string Filename { get; set; }

        [Required]
        [MaxLength(255)]
        [DisplayName("Image alternative text")]
        [Description("How should this image be described to someone who can't see it?")]
        public string ImageAltText { get; set; }

        [NotMapped]
        public string ImageFilePath { get; set; }

        [NotMapped]
        public string ImageLinkPath { get; set; }

        public Language Language { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }

        public NavBanner NavBanner { get; set; }

        [Key]
        [Required]
        public int NavBannerId { get; set; }
    }
}