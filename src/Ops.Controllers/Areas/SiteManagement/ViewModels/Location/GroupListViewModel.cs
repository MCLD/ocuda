using System.Collections.Generic;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class GroupListViewModel
    {
        public IEnumerable<Promenade.Models.Entities.Group> Groups { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}