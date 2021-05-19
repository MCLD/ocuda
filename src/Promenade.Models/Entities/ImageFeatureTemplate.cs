using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ImageFeatureTemplate
    {
        public int? Height { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        public int? ItemsToDisplay { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        public int? Width { get; set; }
    }
}