﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class CarouselItem
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int CarouselId { get; set; }
        public Carousel Carousel { get; set; }

        public int Order { get; set; }

        public ICollection<CarouselButton> Buttons { get; set; }

        [NotMapped]
        public CarouselItemText CarouselItemText { get; set; }

        [NotMapped]
        public string Name { get; set; }
    }
}
