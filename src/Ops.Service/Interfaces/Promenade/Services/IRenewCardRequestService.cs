using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IRenewCardRequestService
    {
        Task<RenewCardRequest> GetRequestAsync(int id);
        Task<int> GetRequestCountAsync(bool? isProcessed);
        Task<CollectionWithCount<RenewCardRequest>> GetRequestsAsync(RequestFilter filter);
    }
}
