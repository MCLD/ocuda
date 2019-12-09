using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Locations
{
    public class LocationViewModel
    {
        public Location Location { get; set; }

        public List<LocationsFeaturesViewModel> LocationFeatures { get; set; }

        public string Address { get; set; }

        public List<Location> NearbyLocations { get; set; }

        public int NearbyCount { get; set; }

        public Group LocationNeighborGroup { get; set; }

        public bool LocationSearchable { get; set; }

        [NotMapped]
        public List<string> StructuredLocationHours { get; set; }
    }
}
