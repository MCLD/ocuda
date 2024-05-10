using System.Collections.Generic;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Feature
{
    public class FeatureViewModel : PaginateModel
    {
        public FeatureViewModel()
        {
            AllFeatures = new List<Promenade.Models.Entities.Feature>();
            AllLanguages = new Dictionary<int, string>();
            FeatureNameLanguages = new List<string>();
            FeatureTextLanguages = new List<string>();
        }

        public ICollection<Promenade.Models.Entities.Feature> AllFeatures { get; }
        public IDictionary<int, string> AllLanguages { get; }
        public bool CanEditSegments { get; set; }
        public bool CanManageFeatures { get; set; }
        public bool CanUpdateSlug { get; set; }
        public Promenade.Models.Entities.Feature Feature { get; set; }
        public ICollection<string> FeatureNameLanguages { get; }
        public ICollection<string> FeatureTextLanguages { get; }

        public IEnumerable<string> NameLanguages { get; set; }

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