using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationFeature
    {
        [Key]
        [Required]
        public int LocationId { get; set; }

        [Key]
        [Required]
        public int FeatureId { get; set; }

        public string Text { get; set; }

        [MaxLength(255)]
        public string RedirectUrl { get; set; }
    }
}
