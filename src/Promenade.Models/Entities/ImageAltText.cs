using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ImageAltText
    {
        [Key]
        public int ImageId { get; set; }
        [Key]
        public int LanguageId { get; set; }
        public Language Language { get; set; }
        public string AltText { get; set; }
    }
}
