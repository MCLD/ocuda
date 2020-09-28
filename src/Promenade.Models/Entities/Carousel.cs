using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Carousel
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public ICollection<CarouselItem> Items { get; set; }

        [NotMapped]
        public CarouselText CarouselText { get; set; }

        [NotMapped]
        public string Name { get; set; }
    }
}
