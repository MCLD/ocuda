using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IExternalResourcePromService
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
