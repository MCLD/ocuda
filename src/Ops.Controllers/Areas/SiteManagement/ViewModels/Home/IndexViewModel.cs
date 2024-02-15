namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Home
{
    public class IndexViewModel
    {
        public bool HasEmediaPermissions { get; set; }
        public bool HasNavigationPermissions { get; set; }
        public bool HasPagePermissions { get; set; }

        public bool HasPermissions
        {
            get
            {
                return IsSiteManager
                    || HasEmediaPermissions
                    || HasPagePermissions
                    || HasPodcastPermissions
                    || HasProductPermissions;
            }
        }

        public bool HasPodcastPermissions { get; set; }
        public bool HasProductPermissions { get; set; }
        public bool IsSiteManager { get; set; }
        public bool CanOptimizeImages { get; set; }
    }
}
