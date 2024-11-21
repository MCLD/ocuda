using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays
{
    public class CurrentDisplayAssetsViewModel
    {
        public IEnumerable<Models.DigitalDisplayCurrentAsset> Assets { get; set; }
        public DigitalDisplay Display { get; set; }
        public bool HasPermissions { get; set; }
        public string LocationName { get; set; }
        public string LocationSlug { get; set; }

        public static string CssIsEnabled(bool enabled)
        {
            return enabled
                ? "text-success"
                : "text-danger";
        }

        public static string HtmlIsEnabled(bool enabled)
        {
            return enabled
                ? "<span class=\"fa-regular fa-square-check\"></span>"
                : "<span class=\"fa-regular fa-circle-xmark\"></span>";
        }
    }
}