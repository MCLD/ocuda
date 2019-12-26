using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IExternalResourceRepository : IRepository<ExternalResource, int>
    {
        Task<ICollection<ExternalResource>> GetAllAsync(ExternalResourceType? type);
        Task<DataWithCount<ICollection<ExternalResource>>> GetPaginatedListAsync(
            ExternalResourceFilter filter);
        Task<int?> GetMaxSortOrderAsync(ExternalResourceType type);
        Task<ExternalResource> GetBySortOrderAsync(ExternalResourceType type, int sortOrder);
        Task<List<ExternalResource>> GetSubsequentAsync(ExternalResourceType type, int sortOrder);
    }
}
