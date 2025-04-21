using System;
using Microsoft.AspNetCore.Http;

namespace Ocuda.Ops.Controllers.ViewModels.Locations
{
    public class ExteriorImageViewModel
    {
        public int CropHeight { get; set; }

        public int CropWidth { get; set; }
         
        public string Filename { get; set; }

        public IFormFile Image { get; set; }

        public string LocationName { get; set; }

        public string Slug { get; set; }

        public static string Now()
        {
            return DateTime.Now.ToString("s");
        }
    }
}