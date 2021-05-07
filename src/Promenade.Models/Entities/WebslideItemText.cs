using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class WebslideItemText
    {
        [DisplayName("Image alternative text")]
        [Description("How should this image be described to someone who can't see it?")]
        [MaxLength(255)]
        [Required]
        public string AltText { get; set; }

        [MaxLength(255)]
        public string Filename { get; set; }

        public Language Language { get; set; }
        public int LanguageId { get; set; }

        [MaxLength(255)]
        [Required]
        public string Link { get; set; }

        public WebslideItem WebslideItem { get; set; }
        public int WebslideItemId { get; set; }
    }
}