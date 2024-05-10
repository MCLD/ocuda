using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Locations
{
    public class LocationFeatureViewModel
    {
        public LocationFeatureViewModel()
        {
            AllLanguages = new Dictionary<int, string>();
            FeatureNameLanguages = new List<string>();
            FeatureTextLanguages = new List<string>();
            LocationFeatureLanguages = new List<string>();
        }

        public IDictionary<int, string> AllLanguages { get; }
        public bool CanEditSegments { get; set; }
        public bool CanManageLocations { get; set; }
        public Feature Feature { get; set; }
        public ICollection<string> FeatureNameLanguages { get; }
        public string FeatureText { get; set; }
        public ICollection<string> FeatureTextLanguages { get; }

        public Location Location { get; set; }

        public LocationFeature LocationFeature { get; set; }

        public ICollection<string> LocationFeatureLanguages { get; }

        public static string GetLanguageTitle(ICollection<string> languages)
        {
            return languages?.Count > 0
                ? $"Available in {languages.Count} {(languages?.Count == 1 ? "language" : "languages")}: {string.Join(", ", languages)}"
                : "No text provided.";
        }

        public string GetLanguageCssClass(ICollection<string> languages)
        {
            return languages?.Count > 0
                ? languages?.Count < AllLanguages.Count ? "text-warning" : "text-success"
                : "text-secondary";
        }
    }
}