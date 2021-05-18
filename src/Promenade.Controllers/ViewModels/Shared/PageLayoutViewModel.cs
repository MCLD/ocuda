using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Shared
{
    public class PageLayoutViewModel
    {
        public PageLayout PageLayout { get; set; }
        public string CanonicalUrl { get; set; }
        public bool HasCarousels { get; set; }
        public string Stub { get; set; }
        public ImageFeatureTemplate PageFeatureTemplate { get; set; }
    }
}