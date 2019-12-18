using System.Collections.Generic;
using System.ComponentModel;
using Ocuda.Ops.Service.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class LocationViewModel
    {
        public ICollection<Promenade.Models.Entities.Location> AllLocations { get; set; }

        public Promenade.Models.Entities.Location Location { get; set; }
        public Promenade.Models.Entities.LocationFeature LocationFeature { get; set; }
        public Promenade.Models.Entities.LocationGroup LocationGroup { get; set; }
        public List<Promenade.Models.Entities.LocationHours> AllLocationHours { get; set; }
        public List<Promenade.Models.Entities.LocationFeature> LocationFeatures { get; set; }
        public List<Promenade.Models.Entities.Feature> Features { get; set; }
        public string FeatureList { get; set; }
        public Promenade.Models.Entities.Feature Feature { get; set; }
        public List<Promenade.Models.Entities.LocationGroup> LocationGroups { get; set; }
        public List<Promenade.Models.Entities.Group> Groups { get; set; }
        public string GroupList { get; set; }
        public Promenade.Models.Entities.Group Group { get; set; }
        public string Action { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public bool IsLocationsGroup { get; set; }
    }
}
