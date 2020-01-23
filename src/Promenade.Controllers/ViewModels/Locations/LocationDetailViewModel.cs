using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Locations
{
    public class LocationDetailViewModel
    {
        public Location Location { get; set; }
        public List<LocationsFeaturesViewModel> LocationFeatures { get; set; }
        public List<LocationGroup> NearbyLocations { get; set; }
        public int NearbyCount { get; set; }
        public Group LocationNeighborGroup { get; set; }
        public List<string> StructuredLocationHours { get; set; }
        public SegmentText PreFeatureSegment { get; set; }
        public SegmentText PostFeatureSegment { get; set; }
    }
}
