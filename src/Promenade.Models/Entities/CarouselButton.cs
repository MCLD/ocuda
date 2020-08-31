using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class CarouselButton
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int CarouselItemId { get; set; }
        public CarouselItem CarouselItem { get; set; }

        public int Order { get; set; }

        [Required]
        [MaxLength(255)]
        public string Url { get; set; }

        [DisplayName("Label")]
        public int LabelId { get; set; }
        public CarouselButtonLabel Label { get; set; }
    }
}
