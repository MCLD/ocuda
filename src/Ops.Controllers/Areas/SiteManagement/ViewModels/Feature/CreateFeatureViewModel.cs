using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Feature
{
    public class CreateFeatureViewModel
    {
        [Required]
        [MaxLength(48)]
        public string Icon { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(80)]
        public string Slug { get; set; }
    }
}