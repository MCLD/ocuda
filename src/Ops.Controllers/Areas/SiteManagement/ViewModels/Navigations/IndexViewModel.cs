using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Navigations
{
    public class IndexViewModel
    {
        public ICollection<Navigation> TopLevelNavigations { get; set; }
        public int TopNavigationId { get; set; }
        public int MiddleNavigationId { get; set; }
        public int LeftNavigationId { get; set; }
        public int FooterNavigationId { get; set; }

        public Navigation Navigation { get; set; }
    }
}
