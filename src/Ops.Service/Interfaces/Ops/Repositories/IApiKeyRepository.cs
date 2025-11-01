using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IApiKeyRepository : IOpsRepository<ApiKey, int>
    {
        Task<ApiKey> FindByKeyAsync(byte[] apiKey);

        Task<CollectionWithCount<ApiKey>> PageAsync(BaseFilter filter);
    }
}