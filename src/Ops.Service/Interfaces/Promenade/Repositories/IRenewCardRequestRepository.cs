using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IRenewCardRequestRepository : IGenericRepository<RenewCardRequest>
    {
        Task<RenewCardRequest> GetByIdAsync(int id);
        Task<int> GetCountAsync(bool? isProcessed);
        Task<CollectionWithCount<RenewCardRequest>> GetPaginatedAsync(RequestFilter filter);
    }
}
