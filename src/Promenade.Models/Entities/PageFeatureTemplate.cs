﻿using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageFeatureTemplate
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public int? Height { get; set; }

        public int? Width { get; set; }

        public int? ItemsToDisplay { get; set; }
    }
}
