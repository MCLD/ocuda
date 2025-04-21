using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.ExternalResources
{
    public class IndexViewModel
    {
        public ICollection<ExternalResource> ExternalResources { get; set; }
        public ExternalResourceType Type { get; set; }
        public ExternalResource ExternalResource { get; set; }
    }
}
