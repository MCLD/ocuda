using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IIdentityProviderRepository : IOpsRepository<IdentityProvider, int>
    {
        Task<int> CountAsync(BaseFilter filter);

        Task<IEnumerable<IdentityProvider>> PageAsync(BaseFilter filter);
    }
}