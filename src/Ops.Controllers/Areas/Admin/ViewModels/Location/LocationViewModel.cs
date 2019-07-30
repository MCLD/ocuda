using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Location
{
    public class LocationViewModel
    {
        public List<Promenade.Models.Entities.Location> AllLocations { get; set; }

        public Promenade.Models.Entities.Location Location { get; set; }
    }
}
