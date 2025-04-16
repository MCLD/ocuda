using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Users
{
    public class PermissionViewModel
    {
        public ICollection<PermissionGroup> PermissionGroups { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public PermissionGroup PermissionGroup { get; set; }
    }
}