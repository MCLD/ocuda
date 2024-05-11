using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Feature
{
    public class SlugViewModel
    {
        [Required]
        public string PriorSlug { get; set; }

        [Required]
        [MaxLength(80)]
        public string Slug { get; set; }
    }
}