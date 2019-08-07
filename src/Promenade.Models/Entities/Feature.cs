﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Feature
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(48)]
        public string FontAwesome { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [MaxLength(80)]
        public string Stub { get; set; }

        public string BodyText { get; set; }

        [NotMapped]
        public bool IsNewFeature { get; set; }
    }
}
