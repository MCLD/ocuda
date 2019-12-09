using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class Feature : BaseEntity
    {
        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(48)]
        [Required]
        public string FontAwesome { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [MaxLength(80)]
        public string Stub { get; set; }

        public string BodyText { get; set; }

        [NotMapped]
        public bool IsNewFeature { get; set; }

        [NotMapped]
        public bool NeedsPopup { get; set; }
    }
}
