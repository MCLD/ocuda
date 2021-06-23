using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class ImageFeatureItemText
    {
        [DisplayName("Image alternative text")]
        [Description("How should this image be described to someone who can't see it?")]
        [MaxLength(255)]
        [Required]
        public string AltText { get; set; }

        [MaxLength(255)]
        public string Filename { get; set; }

        [NotMapped]
        public string Filepath { get; set; }

        public ImageFeatureItem ImageFeatureItem { get; set; }
        public int ImageFeatureItemId { get; set; }
        public Language Language { get; set; }
        public int LanguageId { get; set; }

        [MaxLength(255)]
        [Required]
        public string Link { get; set; }
    }
}