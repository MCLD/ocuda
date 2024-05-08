using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Locations
{
    public class AddFeatureViewModel
    {
        public AddFeatureViewModel()
        {
            AvailableFeatures = new List<Feature>();
        }

        public ICollection<Feature> AvailableFeatures { get; }
        public int FeatureId { get; set; }
        public Location Location { get; set; }
    }
}