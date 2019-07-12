using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels
{
    public class LocationViewModel
    {
        public Location Location { get; set; }

        public List<LocationsFeaturesViewModel> LocationFeatures { get; set; }
    }
}
