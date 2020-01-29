using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationFeature
    {
        [Key]
        [Required]
        public int LocationId { get; set; }

        public Location Location { get; set; }

        [Key]
        [Required]
        public int FeatureId { get; set; }

        public Feature Feature { get; set; }

        public string Text { get; set; }

        [MaxLength(255)]
        public string RedirectUrl { get; set; }

        [DisplayName("Open in new tab")]
        public bool NewTab { get; set; }
    }
}
