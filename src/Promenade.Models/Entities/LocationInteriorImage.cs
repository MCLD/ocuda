using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationInteriorImage
    {
        [NotMapped]
        public List<LocationInteriorImageAltText> AllAltTexts { get; set; }

        [NotMapped]
        public string AltText { get; set; }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string ImagePath { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required]
        public int SortOrder { get; set; }
    }
}