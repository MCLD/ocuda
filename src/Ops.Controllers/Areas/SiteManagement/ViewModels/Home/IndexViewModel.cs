namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Home
{
    public class IndexViewModel
    {
        public bool IsSiteManager { get; set; }
        public bool HasEmediaPermissions { get; set; }
        public bool HasPagePermissions { get; set; }
        public bool HasPodcastPermissions { get; set; }
        public bool HasPermissions
        {
            get
            {
                return IsSiteManager || HasEmediaPermissions || HasPagePermissions 
                    || HasPodcastPermissions;
            }
        }
    }
}
