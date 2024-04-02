using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationInteriorImageAltText
    {
        [MaxLength(255)]
        [Required]
        public string AltText { get; set; }

        public Language Language { get; set; }

        [Key]
        public int LanguageId { get; set; }

        [Key]
        public int LocationInteriorImageId { get; set; }
    }
}