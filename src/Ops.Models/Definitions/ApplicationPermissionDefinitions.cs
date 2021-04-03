using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Models.Keys;

namespace Ocuda.Ops.Models.Definitions
{
    public static class ApplicationPermissionDefinitions
    {
        public static readonly ApplicationPermissionDefinition[] ApplicationPermissions =
            {
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.CoverIssueManagement,
                Name = "Cover Issue Management",
                Info = "Users with this permission are able to mark cover issues as resolved."
            },
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.DigitalDisplayContentManagement,
                Name = "Digital Display Content Management",
                Info = "Users with this permission can manage assets in digital display sets."
            }
        };
    }
}