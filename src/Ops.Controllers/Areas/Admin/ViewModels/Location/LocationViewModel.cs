﻿using System.Collections.Generic;
using Ocuda.Ops.Service.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Location
{
    public class LocationViewModel
    {
        public ICollection<Promenade.Models.Entities.Location> AllLocations { get; set; }

        public Promenade.Models.Entities.Location Location { get; set; }
        public List<Promenade.Models.Entities.LocationFeature> LocationFeatures { get; set; }
        public List<Promenade.Models.Entities.LocationFeature> NonLocationFeatures { get; set; }
        public List<Promenade.Models.Entities.Feature> Features { get; set; }
        public List<Promenade.Models.Entities.Feature> NonFeatures { get; set; }
        public Promenade.Models.Entities.Feature Feature { get; set; }
        public List<Promenade.Models.Entities.LocationGroup> LocationGroups { get; set; }
        public List<Promenade.Models.Entities.LocationGroup> NonLocationGroups { get; set; }
        public List<Promenade.Models.Entities.Group> Groups { get; set; }
        public List<Promenade.Models.Entities.Group> NonGroups { get; set; }
        public Promenade.Models.Entities.Group Group { get; set; }
        public string Action { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
