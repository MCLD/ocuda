namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Home
{
    public class IndexViewModel
    {
        public bool HasEmediaPermissions { get; set; }
        public bool HasFeatureManagement { get; set; }
        public bool HasImageOptimizePermissions { get; set; }
        public bool HasNavigationPermissions { get; set; }
        public bool HasPagePermissions { get; set; }

        public bool HasPermissions
        {
            get
            {
                return IsSiteManager
                    || HasEmediaPermissions
                    || HasFeatureManagement
                    || HasImageOptimizePermissions
                    || HasPagePermissions
                    || HasPodcastPermissions
                    || HasProductPermissions;
            }
        }

        public bool HasPodcastPermissions { get; set; }
        public bool HasProductPermissions { get; set; }
        public bool IsSiteManager { get; set; }
    }
}