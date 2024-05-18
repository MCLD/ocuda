using System;
using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Locations
{
    public class DetailsViewModel
    {
        public DetailsViewModel()
        {
            DescriptionLanguages = new List<string>();
            AllLanguages = new Dictionary<int, string>();
        }

        public IDictionary<int, string> AllLanguages { get; }

        public IEnumerable<Feature> AtThisLocation { get; set; }

        public ICollection<string> DescriptionLanguages { get; }

        public string LanguageCssClass
        {
            get
            {
                return DescriptionLanguages.Count < AllLanguages.Count
                    ? "text-warning"
                    : "text-success";
            }
        }

        public string LanguageTitle
        {
            get
            {
                return DescriptionLanguages.Count == 1
                    ? $"Available in {DescriptionLanguages.Count} language: {string.Join(", ", DescriptionLanguages)}"
                    : $"Available in {DescriptionLanguages.Count} langauges:  {string.Join(", ", DescriptionLanguages)}";
            }
        }

        public Location Location { get; set; }

        public bool LocationManager { get; set; }

        public bool SegmentEditor { get; set; }

        public IEnumerable<Feature> ServicesAvailable { get; set; }

        public static string Now()
        {
            return DateTime.Now.ToString("s");
        }
    }
}