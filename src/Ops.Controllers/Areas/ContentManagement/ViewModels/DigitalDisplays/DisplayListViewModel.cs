using System.Collections.Generic;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays
{
    public class DisplayListViewModel
    {
        public ICollection<Models.Entities.DigitalDisplay> DigitalDisplays { get; set; }
        public IDictionary<int, string> DisplaySetNames { get; set; }
        public IDictionary<int, string> Locations { get; set; }

        public static string RowClass(Models.Entities.DigitalDisplay display)
        {
            if (display != null)
            {
                if (System.DateTime.Now.AddHours(-2) < display.LastContentVerification)
                {
                    return "table-warning";
                }
                else if (System.DateTime.Now.AddHours(-4) < display.LastContentVerification)
                {
                    return "table-danger";
                }
            }
            return string.Empty;
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