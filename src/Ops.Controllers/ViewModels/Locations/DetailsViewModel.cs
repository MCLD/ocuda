using System;
using System.Collections.Generic;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location;
using Ocuda.Ops.Models.Entities;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Locations
{
    public class DetailsViewModel
    {
        public DetailsViewModel()
        {
            AllLanguages = new Dictionary<int, string>();
            DescriptionLanguages = [];
            LocationNoticeLanguages = [];
            VolunteerForms = [];
        }

        public static string Now
        {
            get
            {
                return DateTime.Now.ToString("s");
            }
        }

        public string ActiveLocationNoticeBorderCssClass
        {
            get
            {
                return !LocationNoticeSegment.IsActive || IsBeforeStart || IsAfterEnd
                    ? "border-danger"
                    : "border-success";
            }
        }

        public string ActiveLocationNoticeCssClass
        {
            get
            {
                return !LocationNoticeSegment.IsActive || IsBeforeStart || IsAfterEnd
                    ? "text-danger"
                    : "text-success";
            }
        }

        public IDictionary<int, string> AllLanguages { get; }

        public IEnumerable<Feature> AtThisLocation { get; set; }

        public ICollection<string> DescriptionLanguages { get; }

        public IEnumerable<DigitalDisplay> Displays { get; set; }

        public bool IsSiteManager { get; set; }

        public Location Location { get; set; }

        public bool LocationManager { get; set; }

        public ICollection<string> LocationNoticeLanguages { get; }

        public Segment LocationNoticeSegment { get; set; }

        public string LocationNoticeStatus
        {
            get
            {
                return !LocationNoticeSegment.IsActive
                    ? "Disabled"
                        : IsBeforeStart
                        ? $"Starts {LocationNoticeSegment.StartDate}"
                        : IsAfterEnd
                            ? $"Ended {LocationNoticeSegment.EndDate}"
                            : "Live";
            }
        }

        public bool SegmentEditor { get; set; }

        public IEnumerable<Feature> ServicesAvailable { get; set; }

        public ICollection<LocationVolunteerFormViewModel> VolunteerForms { get; }

        private bool IsAfterEnd
        {
            get
            {
                return LocationNoticeSegment.EndDate.HasValue
                    && LocationNoticeSegment.EndDate <= DateTime.Now;
            }
        }

        private bool IsBeforeStart
        {
            get
            {
                return LocationNoticeSegment.StartDate.HasValue
                    && LocationNoticeSegment.StartDate >= DateTime.Now;
            }
        }

        public static string LanguagesTitle(ICollection<string> languages)
        {
            if (languages == null) { return "Not available in any languages."; }
            return languages.Count == 0
                ? $"Available in {languages.Count} languages."
                : languages.Count == 1
                    ? $"Available in {languages.Count} language: {string.Join(", ", languages)}"
                    : $"Available in {languages.Count} languages:  {string.Join(", ", languages)}";
        }

        public string LanguagesCssClass(ICollection<string> languages)
        {
            return languages != null && languages?.Count == AllLanguages.Count
                ? "text-success"
                : "text-warning";
        }
    }
}