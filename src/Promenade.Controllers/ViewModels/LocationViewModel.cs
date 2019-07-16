﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels
{
    public class LocationViewModel
    {
        public Location Location { get; set; }

        public List<LocationsFeaturesViewModel> LocationFeatures { get; set; }

        public string Address { get; set; }
    }
}
