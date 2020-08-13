using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IExternalResourceService
    {
        Task<ICollection<ExternalResource>> GetAllAsync(ExternalResourceType? type);
        Task<DataWithCount<ICollection<ExternalResource>>> GetPaginatedListAsync(
            ExternalResourceFilter filter);
        Task<ExternalResource> AddAsync(ExternalResource resource);
        Task<ExternalResource> EditAsync(ExternalResource resource);
        Task DeleteAsync(int id);
        Task DecreaseSortOrder(int id);
        Task IncreaseSortOrder(int id);
    }
}
