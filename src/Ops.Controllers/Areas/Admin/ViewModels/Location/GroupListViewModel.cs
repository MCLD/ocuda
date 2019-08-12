using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Location
{
    public class GroupListViewModel
    {
        public IEnumerable<Group> Groups { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
