using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Ocuda.Ops.Controllers.ViewModels.Locations
{
    public class InteriorImageViewModel
    {
        public InteriorImageViewModel()
        {
            AltTexts = new Dictionary<int, string>();
            Languages = new Dictionary<int, string>();
        }

        public IDictionary<int, string> AltTexts { get; }
        public int CropHeight { get; set; }
        public int CropWidth { get; set; }
        public string Filename { get; set; }
        public IFormFile Image { get; set; }
        public int? ImageId { get; set; }
        public IDictionary<int, string> Languages { get; }
        public string LocationName { get; set; }
        public string Slug { get; set; }
        public int SortOrder { get; set; }

        public static string Now()
        {
            return DateTime.Now.ToString("s");
        }
    }
}