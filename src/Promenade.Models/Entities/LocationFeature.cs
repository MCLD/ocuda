using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationFeature
    {
        public Feature Feature { get; set; }

        [Key]
        [Required]
        public int FeatureId { get; set; }

        public Location Location { get; set; }

        [Key]
        [Required]
        public int LocationId { get; set; }

        [DisplayName("Open in new tab")]
        public bool NewTab { get; set; }

        [MaxLength(255)]
        public string RedirectUrl { get; set; }

        public int? SegmentId { get; set; }

        [NotMapped]
        public string Text { get; set; }
    }
}