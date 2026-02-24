using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IRenewCardResultRepository : IOpsRepository<RenewCardResult, int>
    {
        Task<RenewCardResult> GetForRequestAsync(int requestId);
        Task<RenewCardResponse.ResponseType> GetRequestResponseTypeAsync(int requestId);
    }
}
