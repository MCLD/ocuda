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
                    || HasSectionManagerPermissions;
            }
        }

        public bool HasRosterPermissions { get; set; }
        public bool HasSectionManagerPermissions { get; set; }
        public bool IsSiteManager { get; set; }
    }
}
