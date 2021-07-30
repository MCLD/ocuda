using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Emedia
{
    public class GroupsViewModel
    {
        public ICollection<EmediaGroup> EmediaGroups { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public EmediaGroup EmediaGroup { get; set; }
    }
}
