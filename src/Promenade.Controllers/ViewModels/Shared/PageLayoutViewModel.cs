using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Shared
{
    public class PageLayoutViewModel
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1056:URI-like properties should not be strings",
            Justification = "URI-property is a string for database storage")]
        public string CanonicalUrl { get; set; }

        public bool HasCarousels { get; set; }
        public ImageFeatureTemplate PageFeatureTemplate { get; set; }
        public PageLayout PageLayout { get; set; }
        public string Stub { get; set; }
    }
}