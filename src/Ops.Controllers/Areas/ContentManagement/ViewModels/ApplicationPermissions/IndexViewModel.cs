using System.Collections.Generic;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.ApplicationPermissions
{
    public class IndexViewModel
    {
        public ICollection<DataWithCount<ApplicationPermissionDefinition>> ApplicationPermissions { get; set; }
    }
}
