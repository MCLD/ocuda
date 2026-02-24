using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IRenewCardResponseRepository : IOpsRepository<RenewCardResponse, int>
    {
        Task<RenewCardResponse> GetBySortOrderAsync(int sortOrder);
        Task<int?> GetMaxSortOrderAsync();
        Task<IEnumerable<RenewCardResponse>> GetAllAsync();
        Task<IEnumerable<RenewCardResponse>> GetAvailableAsync();
        Task<List<RenewCardResponse>> GetSubsequentAsync(int sortOrder);
    }
}
