using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IExternalResourcePromRepository : IGenericRepository<ExternalResource>
    {
        Task<ExternalResource> FindAsync(int id);
        Task<ICollection<ExternalResource>> GetAllAsync(ExternalResourceType? type);
        Task<DataWithCount<ICollection<ExternalResource>>> GetPaginatedListAsync(
            ExternalResourceFilter filter);
        Task<int?> GetMaxSortOrderAsync(ExternalResourceType type);
        Task<ExternalResource> GetBySortOrderAsync(ExternalResourceType type, int sortOrder);
        Task<List<ExternalResource>> GetSubsequentAsync(ExternalResourceType type, int sortOrder);
    }
}
