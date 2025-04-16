using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.ExternalResources
{
    public class IndexViewModel
    {
        public ICollection<ExternalResource> ExternalResources { get; set; }
        public ExternalResourceType Type { get; set; }
        public ExternalResource ExternalResource { get; set; }
    }
}