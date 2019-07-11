using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels
{
    public class LocationsViewModel
    {
        public Locations Location { get; set; }

        public List<LocationsFeaturesViewModel> LocationFeatures { get; set; }


    }
}
