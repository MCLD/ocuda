using System.Collections.Generic;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class LocationMapViewModel : LocationPartialViewModel
    {
        public Promenade.Models.Entities.Location Location { get; set; }
        public ICollection<Promenade.Models.Entities.LocationGroup> LocationGroups { get; set; }

        public string MapApiKey { get; set; }
    }
}
