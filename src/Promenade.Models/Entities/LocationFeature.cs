using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationFeature : BaseEntity
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
