using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.IdentityProviders
{
    public class IdentityProvidersViewModel
    {
        public int Count { get; set; }
        public IEnumerable<IdentityProvider> IdentityProviders { get; set; }
    }
}