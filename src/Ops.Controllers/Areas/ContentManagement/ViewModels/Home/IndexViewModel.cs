namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Home
{
    public class IndexViewModel
    {
        public bool HasPermissions
        {
            get
            {
                return IsSiteManager
                    || HasRosterPermissions
                    || HasSectionManagerPermissions
                    || HasUserSyncPermissions;
            }
        }

        public bool HasCardRenewalPermissions { get; set; }
        public bool HasRosterPermissions { get; set; }
        public bool HasSectionManagerPermissions { get; set; }
        public bool HasUserSyncPermissions { get; set; }
        public bool IsSiteManager { get; set; }
    }
}