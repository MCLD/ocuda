using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageFeatureItemText
    {
        [DisplayName("Image alternative text")]
        [Description("How should this image be described to someone who can't see it?")]
        [Required]
        [MaxLength(255)]
        public string AltText { get; set; }

        [MaxLength(255)]
        public string Filename { get; set; }

        public Language Language { get; set; }
        public int LanguageId { get; set; }

        [MaxLength(255)]
        [Required]
        public string Link { get; set; }

        public PageFeatureItem PageFeatureItem { get; set; }
        public int PageFeatureItemId { get; set; }
    }
}