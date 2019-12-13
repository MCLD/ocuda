using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Controllers.ViewModels.Locations
{
    public class LocationsFeaturesViewModel
    {
        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(48)]
        public string Icon { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [MaxLength(80)]
        [Required]
        public string Stub { get; set; }

        [Required]
        public string BodyText { get; set; }

        public string Text { get; set; }

        [MaxLength(255)]
        public string RedirectUrl { get; set; }

        [NotMapped]
        public string InnerSpan { get; set; }
    }
}
