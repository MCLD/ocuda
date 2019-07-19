using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationFeature
    {
        [Required]
        public int LocationId { get; set; }

        [Required]
        public int FeatureId { get; set; }

        public string Text { get; set; }

        [MaxLength(255)]
        public string RedirectUrl { get; set; }
    }
}
