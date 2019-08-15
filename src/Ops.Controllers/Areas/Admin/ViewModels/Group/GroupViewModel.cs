using System.Collections.Generic;
using Ocuda.Ops.Service.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Location
{
    public class GroupViewModel
    {
        public List<Promenade.Models.Entities.Group> AllGroups { get; set; }
        public Promenade.Models.Entities.Group Group { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
