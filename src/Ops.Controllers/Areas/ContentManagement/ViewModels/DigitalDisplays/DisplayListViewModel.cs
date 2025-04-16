using System.Collections.Generic;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays
{
    public class DisplayListViewModel
    {
        public ICollection<Models.Entities.DigitalDisplay> DigitalDisplays { get; set; }
        public IDictionary<int, int> DisplayActiveElements { get; set; }
        public IDictionary<int, string> DisplaySetNames { get; set; }
        public bool HasDigitalDisplayPermissions { get; set; }

        public bool HasPermissions
        {
            get
            {
                return IsSiteManager || HasDigitalDisplayPermissions;
            }
        }

        public bool IsSiteManager { get; set; }
        public IDictionary<int, string> Locations { get; set; }

        public static string RowClass(Models.Entities.DigitalDisplay display)
        {
            if (display?.LastContentVerification.HasValue == true
                && display.LastContentVerification.Value.AddHours(4) >= System.DateTime.Now)
            {
                return display.LastContentVerification.Value.AddHours(2) <= System.DateTime.Now
                    ? "table-warning"
                    : string.Empty;
            }
            return "table-danger";
        }

        public bool HasSets(int displayId)
        {
            return DisplaySetNames.ContainsKey(displayId)
                && !string.IsNullOrEmpty(DisplaySetNames[displayId]);
        }

        public string ShowLocation(int? locationId)
        {
            return locationId != null && Locations.ContainsKey((int)locationId)
                ? Locations[(int)locationId]
                : "unspecified";
        }

        public string ShowSets(int displayId)
        {
            return !string.IsNullOrEmpty(DisplaySetNames?[displayId])
                ? DisplaySetNames?[displayId]
                : "none";
        }
    }
}