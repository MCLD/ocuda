using System.Collections.Generic;
using Ocuda.Ops.Service.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Location
{
    public class LocationViewModel
    {
        public ICollection<Promenade.Models.Entities.Location> AllLocations { get; set; }

        public Promenade.Models.Entities.Location Location { get; set; }
        public bool IsSavedLocation { get; set; }

        public PaginateModel PaginateModel { get; set; }
    }
}
