using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IIdentityProviderService
    {
        Task AddProviderAsync(IdentityProvider provider, string certificate);

        Task DeleteAsync(int providerId);

        Task<IdentityProvider> GetActiveAsync(string slug);

        Task<IEnumerable<IdentityProvider>> GetAllActiveAsync();

        Task<DataWithCount<ICollection<IdentityProvider>>> PageAsync(BaseFilter filter);
    }
}