﻿using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Locations
{
    public class LocationViewModel
    {
        public bool CanSearchAddress { get; set; }
        public string Info { get; set; }
        public double? Latitude { get; set; }
        public ICollection<Location> Locations { get; set; }
        public double? Longitude { get; set; }
        public string Warning { get; set; }
        public string Zip { get; set; }
    }
}