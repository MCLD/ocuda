using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.ExternalResources
{
    public class IndexViewModel
    {
        public ICollection<ExternalResource> ExternalResources { get; set; }
        public ExternalResourceType Type { get; set; }
        public ExternalResource ExternalResource { get; set; }
    }
}
