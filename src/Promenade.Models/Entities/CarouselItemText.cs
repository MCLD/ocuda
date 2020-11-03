using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class CarouselItemText
    {
        public int CarouselItemId { get; set; }
        public CarouselItem CarouselItem { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [Required]
        [MaxLength(100)]
        [DisplayName("Label under image")]
        public string Label { get; set; }

        [Required]
        [DisplayName("Image URL")]
        [MaxLength(255)]
        public string ImageUrl { get; set; }

        [MaxLength(255)]
        [DisplayName("Title in popup")]
        public string Title { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }
    }
}
