using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Locations
{
    public class IndexViewModel : PaginateModel
    {
        public IEnumerable<Location> Locations { get; set; }
    }
}