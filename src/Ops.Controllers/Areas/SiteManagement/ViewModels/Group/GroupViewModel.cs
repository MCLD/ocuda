using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Service.Models;

using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Group
{
    public class GroupViewModel
    {
        public ICollection<Promenade.Models.Entities.Group> AllGroups { get; set; }
        public Promenade.Models.Entities.Group Group { get; set; }
        public List<Promenade.Models.Entities.Group> Groups { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public string Action { get; set; }
    }
}
