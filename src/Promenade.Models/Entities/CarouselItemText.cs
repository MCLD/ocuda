using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class CarouselItemText
    {
        [Display(Name = "Image alternative text",
            Description = "How should this image be described to someone who can't see it?")]
        [MaxLength(255)]
        [Required]
        public string AltText { get; set; }

        public CarouselItem CarouselItem { get; set; }
        public int CarouselItemId { get; set; }

        [MaxLength(2000)]
        public string Description { get; set; }

        [Required]
        [DisplayName("Image URL")]
        [MaxLength(255)]
        public string ImageUrl { get; set; }

        [Required]
        [MaxLength(100)]
        [DisplayName("Label under image")]
        public string Label { get; set; }

        public Language Language { get; set; }
        public int LanguageId { get; set; }

        [MaxLength(255)]
        [DisplayName("Title in popup")]
        public string Title { get; set; }
    }
}