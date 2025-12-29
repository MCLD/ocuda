using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICardRenewalResponseRepository : IOpsRepository<CardRenewalResponse, int>
    {
        Task<CardRenewalResponse> GetBySortOrderAsync(int sortOrder);
        Task<int?> GetMaxSortOrderAsync();
        Task<IEnumerable<CardRenewalResponse>> GetAllAsync();
        Task<IEnumerable<CardRenewalResponse>> GetAvailableAsync();
        Task<List<CardRenewalResponse>> GetSubsequentAsync(int sortOrder);
    }
}
