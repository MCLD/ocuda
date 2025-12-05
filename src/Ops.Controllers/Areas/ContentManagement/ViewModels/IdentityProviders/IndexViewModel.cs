using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.IdentityProviders
{
    public class IndexViewModel : PaginateModel
    {
        public IEnumerable<IdentityProvider> Providers { get; set; }
    }
}