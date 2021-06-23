using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [DisplayName("Link Value")]
        [MaxLength(255)]
        public string Url { get; set; }

        [DisplayName("Button Text")]
        public int LabelId { get; set; }
        public CarouselButtonLabel Label { get; set; }

        [NotMapped]
        public CarouselButtonLabelText LabelText { get; set; }
    }
}
