using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmployeeCardRequestRepository : IGenericRepository<EmployeeCardRequest>
    {
        Task<EmployeeCardRequest> GetByIdAsync(int Id);
        Task<int> GetCountAsync(bool? isProcessed);
        Task<CollectionWithCount<EmployeeCardRequest>> GetPaginatedAsync(RequestFilter filter);
    }
}
