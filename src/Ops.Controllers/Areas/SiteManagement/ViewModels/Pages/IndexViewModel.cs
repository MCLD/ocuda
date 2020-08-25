using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Pages
{
    public class IndexViewModel
    {
        public ICollection<PageHeader> PageHeaders { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public PageType PageType { get; set; }
        public PageHeader PageHeader { get; set; }
        public bool IsSiteManager { get; set; }
        public IList<string> PermissionIds { get; set; }
    }
}
