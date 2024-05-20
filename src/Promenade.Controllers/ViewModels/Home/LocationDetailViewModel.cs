using System;
using System.Collections.Generic;
using System.Linq;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Home
{
    public class LocationDetailViewModel
    {
        public LocationDetailViewModel()
        {
            NearbyLocationGroups = new List<LocationGroup>();
        }

        public IEnumerable<LocationsFeaturesViewModel> AtThisLocation { get; set; }
        public string CanonicalLink { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string DescriptionSegmentText { get; set; }
        public string HoursSegmentText { get; set; }
        public Location Location { get; set; }
        public IEnumerable<LocationsFeaturesViewModel> LocationFeatures { get; set; }
        public Group LocationNeighborGroup { get; set; }
        public int NearbyCount { get; set; }
        public int NearbyEventsCount { get; set; }
        public ICollection<LocationGroup> NearbyLocationGroups { get; }
        public string PostFeatureSegmentHeader { get; set; }
        public string PostFeatureSegmentText { get; set; }
        public string PreFeatureSegmentHeader { get; set; }
        public string PreFeatureSegmentText { get; set; }
        public Schema.NET.Thing Schema { get; set; }
        public string SeeServicesAtAllLink { get; set; }
        public IEnumerable<LocationsFeaturesViewModel> ServicesAvailable { get; set; }
        public string ShowMessage { get; set; }

        public string SocialIcon { get; set; }
        public string SocialLink { get; set; }
        public string SocialName { get; set; }

        public string ActiveIfFirstInteriorImage(int itemSortOrder) =>
            Location?.InteriorImages?.Min(_ => _.SortOrder) == itemSortOrder
                ? "active"
                : null;
    }
}