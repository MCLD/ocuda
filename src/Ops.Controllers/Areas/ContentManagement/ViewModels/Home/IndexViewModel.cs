namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Home
{
    public class IndexViewModel
    {
        public bool HasDigitalDisplayPermissions { get; set; }

        public bool HasPermissions
        {
            get
            {
                return HasSectionManagerPermissions
                    || IsSiteManager
                    || HasDigitalDisplayPermissions;
            }
        }

        public bool HasSectionManagerPermissions { get; set; }
        public bool IsSiteManager { get; set; }
    }
}