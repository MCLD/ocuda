using System;
using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Locations
{
    public class InteriorImagesViewModel
    {
        public IEnumerable<LocationInteriorImage> InteriorImages { get; set; }
        public string LocationName { get; set; }

        public string Slug { get; set; }

        public static string Now()
        {
            return DateTime.Now.ToString("s");
        }
    }
}