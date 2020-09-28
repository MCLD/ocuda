using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class CarouselText
    {
        [Key]
        [Required]
        public int CarouselId { get; set; }
        public Carousel Carousel { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
    }
}
