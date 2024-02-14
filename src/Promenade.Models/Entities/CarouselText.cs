using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class CarouselText
    {
        [Key]
        [Required]
        public int CarouselId { get; set; }
        public Carousel Carousel { get; set; }

        [MaxLength(255)]
        [DisplayName("Footer (optional)")]
        public string Footer { get; set; }
        [MaxLength(255)]
        [DisplayName("Footer Link (optional)")]
        public string FooterLink { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [Required]
        [MaxLength(255)]
        public string Title { get; set; }
    }
}
