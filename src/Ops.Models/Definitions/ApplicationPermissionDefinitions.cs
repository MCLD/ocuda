using System.Collections.Generic;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Models.Keys;

namespace Ocuda.Ops.Models.Definitions
{
    public static class ApplicationPermissionDefinitions
    {
        public static readonly List<ApplicationPermissionDefinition> ApplicationPermissions
            = new List<ApplicationPermissionDefinition>
        {
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.CoverIssueManagement,
                Name = "Cover Issue Management",
                Info = "Users with this permission are able to mark cover issues as resolved."
            }
        };
    }
}
