using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class LocationHoursViewModel : LocationPartialViewModel
    {
        public int LocationId { get; set; }

        public LocationHoursOverride AddOverride { get; set; }
        public LocationHoursOverride EditOverride { get; set; }
        public LocationHoursOverride DeleteOverride { get; set; }

        public List<LocationHours> LocationHours { get; set; }
        public ICollection<LocationHoursOverride> LocationHoursOverrides { get; set; }
    }
}