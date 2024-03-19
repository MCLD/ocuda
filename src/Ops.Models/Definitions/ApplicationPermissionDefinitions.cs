using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Models.Keys;

namespace Ocuda.Ops.Models.Definitions
{
    public static class ApplicationPermissionDefinitions
    {
        public static readonly ApplicationPermissionDefinition[] ApplicationPermissions =
            {
            new() {
                Id = ApplicationPermission.CoverIssueManagement,
                Info = "Able to mark cover issues as resolved.",
                Name = "Cover Issue Management"
            },
            new() {
                Id = ApplicationPermission.DigitalDisplayContentManagement,
                Info = "Manage assets in digital display sets.",
                Name = "Digital Display Content Management"
            },
            new() {
                Id = ApplicationPermission.EmediaManagement,
                Info = "Manage the Promenade Emedia page.",
                Name = "Emedia Management"
            },
            new() {
                Id = ApplicationPermission.ImageOptimizer,
                Info = "Ability to directly access the image optimizer.",
                Name = "Image Optimizer access"
            },
            new() {
                Id = ApplicationPermission.IntranetFrontPageManagement,
                Info = "Push section posts to the front page and pin them.",
                Name = "Intranet Front Page Management"
            },
            new() {
                Id = ApplicationPermission.NavigationManagement,
                Info = "Manage the Promenade navigations.",
                Name = "Navigation Management"
            },
            new() {
                Id = ApplicationPermission.MultiUserAccount,
                Info = "This permission indicates the user is accessing the site via a multi-user domain account.",
                Name = "Multi-user Account"
            },
            new() {
                Id = ApplicationPermission.PodcastShowNotesManagement,
                Info = "Add and edit podcast show notes.",
                Name = "Podcast Show Notes Management"
            },
            new() {
                Id = ApplicationPermission.RosterManagement,
                Info = "Upload rosters and manage mapping units to locations.",
                Name = "Roster upload and unit mapping management"
            },
            new() {
                Id = ApplicationPermission.UpdateProfilePictures,
                Info = "Update profile pictures for all users.",
                Name = "Update Profile Pictures"
            },
            new() {
                Id = ApplicationPermission.UserSync,
                Info = "Able to synchronize users and locations with LDAP/AD.",
                Name = "User sync with LDAP/AD"
            },
            new() {
                Id = ApplicationPermission.ViewAllIncidentReports,
                Info = "View all incident reports, not just their own.",
                Name = "View All Incident Reports"
            },
            new() {
                Id = ApplicationPermission.WebPageContentManagement,
                Info = "Edit all pages.",
                Name = "Web Page Content Management"
            }
        };
    }
}