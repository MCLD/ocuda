using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Locations
{
    public class LocationViewModel
    {
        public ICollection<Location> Locations { get; set; }

        public string Zip { get; set; }

        public bool CanSearchAddress { get; set; }

        public bool IsMobile { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
