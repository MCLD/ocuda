using System.Collections.Generic;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class LocationViewModel : LocationPartialViewModel
    {
        public string Action { get; set; }

        public ICollection<Promenade.Models.Entities.Location> AllLocations { get; set; }

        public string DescriptionSegmentName { get; set; }
        public Promenade.Models.Entities.Feature Feature { get; set; }
        public string FeatureList { get; set; }
        public ICollection<Promenade.Models.Entities.Feature> Features { get; set; }
        public Promenade.Models.Entities.Group Group { get; set; }
        public string GroupList { get; set; }
        public ICollection<Promenade.Models.Entities.Group> Groups { get; set; }
        public string HoursSegmentName { get; set; }
        public bool IsLocationsGroup { get; set; }
        public Promenade.Models.Entities.Location Location { get; set; }
        public Promenade.Models.Entities.LocationFeature LocationFeature { get; set; }
        public ICollection<Promenade.Models.Entities.LocationFeature> LocationFeatures { get; set; }
        public Promenade.Models.Entities.LocationGroup LocationGroup { get; set; }
        public ICollection<Promenade.Models.Entities.LocationGroup> LocationGroups { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public string PostFeatureSegmentName { get; set; }
        public string PreFeatureSegmentName { get; set; }

        public string SocialCardName { get; set; }
        public string StaffSearchLink { get; set; }

        public List<LocationVolunteerFormViewModel> VolunteerForms { get; set; }
    }
}