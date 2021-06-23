namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Home
{
    public class IndexViewModel
    {
        public bool HasDigitalDisplayPermissions { get; set; }

        public bool HasPermissions
        {
            get
            {
                return IsSiteManager || HasDigitalDisplayPermissions;
            }
        }

        public bool IsSiteManager { get; set; }
    }
}