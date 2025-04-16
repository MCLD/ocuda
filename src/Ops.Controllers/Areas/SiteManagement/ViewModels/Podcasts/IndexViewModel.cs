using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Podcasts
{
    public class IndexViewModel
    {
        public ICollection<Podcast> Podcasts { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public bool IsSiteManager { get; set; }
        public IList<string> PermissionIds { get; set; }
    }
}