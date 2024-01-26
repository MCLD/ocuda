using System;
using System.Collections.Generic;
using System.Linq;
using ImageOptimApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.ImageOptimizer
{
    public class ImageOptimizerViewModel
    {
        public IFormFile FormFile { get; set; }
        public Format TargetFormat { get; set; } = Format.Auto;
        public static ICollection<SelectListItem> Formats
        {
            get
            {
                return Enum.GetNames(typeof(Format))
                    .Where(_ => _ != Format.H264.ToString() && _ != Format.WebM.ToString())
                    .Select((_, index) => new SelectListItem { Text = _, Value = index.ToString() }).ToList();
            }
        }
    }
}
