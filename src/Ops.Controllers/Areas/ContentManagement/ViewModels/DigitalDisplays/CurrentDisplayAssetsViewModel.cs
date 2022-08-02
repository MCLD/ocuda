using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays
{
    public class CurrentDisplayAssetsViewModel
    {
        public IEnumerable<Models.DigitalDisplayCurrentAsset> Assets { get; set; }
        public DigitalDisplay Display { get; set; }
        public bool HasPermissions { get; set; }

        public static string CssIsEnabled(bool enabled)
        {
            return enabled
                ? "text-success"
                : "text-danger";
        }

        public static string HtmlIsEnabled(bool enabled)
        {
            return enabled
                ? "<span class=\"far fa-check-square\"></span>"
                : "<span class=\"far fa-times-circle\"></span>";
        }
    }
}
