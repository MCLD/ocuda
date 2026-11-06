using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.ImageOptimizer
{
    public class ImageOptimizerViewModel
    {
        public IFormFile FormFile { get; set; }
        public string TargetFormat { get; set; }

        public static ICollection<SelectListItem> Formats
        {
            get
            {
                return
                [
                    new SelectListItem("Automatic", null),
                    new SelectListItem("JPEG", System.Net.Mime.MediaTypeNames.Image.Jpeg),
                    new SelectListItem("PNG", System.Net.Mime.MediaTypeNames.Image.Png),
                ];
            }
        }
    }
}
