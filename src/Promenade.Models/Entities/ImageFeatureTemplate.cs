using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class ImageFeatureTemplate
    {
        [DisplayName("Height (px)")]
        public int? Height { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [DisplayName("Items to display")]
        public int? ItemsToDisplay { get; set; }

        [DisplayName("Maximum file size (bytes)")]
        public int? MaximumFileSizeBytes { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [DisplayName("Width (px)")]
        public int? Width { get; set; }
    }
}
