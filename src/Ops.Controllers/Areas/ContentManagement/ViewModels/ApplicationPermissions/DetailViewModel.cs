using System.Collections.Generic;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.ApplicationPermissions
{
    public class DetailViewModel
    {
        public ApplicationPermissionDefinition ApplicationPermission { get; set; }
        public int PermissionGroupId { get; set; }

        public ICollection<PermissionGroup> AssignedGroups { get; set; }
        public ICollection<PermissionGroup> AvailableGroups { get; set; }
    }
}
