using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IEmployeeCardService
    {
        Task<EmployeeCardRequest> GetRequestAsync(int requestId);
        Task<int> GetRequestCountAsync(bool? isProcessed);
        Task<CollectionWithCount<EmployeeCardRequest>> GetRequestsAsync(EmployeeCardFilter filter);
        Task UpdateNotesAsync(EmployeeCardRequest cardRequest);
    }
}
