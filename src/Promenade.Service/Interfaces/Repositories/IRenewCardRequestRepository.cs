using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IRenewCardRequestRepository : IGenericRepository<RenewCardRequest>
    {
        Task AddSaveAsync(RenewCardRequest request);
        Task<RenewCardRequest> GetPendingRequestAsync(int customerId);
    }
}
