using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class CarouselButtonLabelText
    {
        [Key]
        [Required]
        public int CarouselButtonLabelId { get; set; }
        public CarouselButtonLabel CarouselButtonLabel { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [Required]
        [MaxLength(32)]
        public string Text { get; set; }
    }
}
