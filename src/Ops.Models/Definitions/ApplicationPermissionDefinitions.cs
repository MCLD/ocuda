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
            },
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.EmediaManagement,
                Name = "Emedia Management",
                Info = "Users with this permission can manage the Promenade Emedia page."
            },
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.IntranetFrontPageManagement,
                Name = "Intranet Front Page Management",
                Info = "Users with this permission can push section posts to the front page and pin them."
            },
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.NavigationManagement,
                Name = "Navigation Management",
                Info = "Users with this permission can manage the Promenade navigations."
            },
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.MultiUserAccount,
                Name = "Multi-user Account",
                Info = "Users with this permission are accessing via a multi-user domain account."
            },
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.PodcastShowNotesManagement,
                Name = "Podcast Show Notes Management",
                Info = "Users with this permission can add and edit podcast show notes."
            },
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.RosterManagement,
                Name = "Roster upload and unit mapping management",
                Info = "Users with this permission can upload rosters and manage mapping units to locations."
            },
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.UpdateProfilePictures,
                Name = "Update Profile Pictures",
                Info = "Users with this permission can update profile pictures for all users."
            },
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.ViewAllIncidentReports,
                Name = "View All Incident Reports",
                Info = "Users with this permission can view all incident reports, not just their own."
            },
            new ApplicationPermissionDefinition
            {
                Id = ApplicationPermission.WebPageContentManagement,
                Name = "Web Page Content Management",
                Info = "Users with this permission can edit all pages."
            }
        };
    }
}
