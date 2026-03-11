using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IEmployeeCardRequestService
    {
        Task<string> GetDepartmentNameAsync(int departmentId);
        Task<EmployeeCardRequest> GetRequestAsync(int id);
        Task<int> GetRequestCountAsync();
        Task<CollectionWithCount<EmployeeCardRequest>> GetRequestsAsync(BaseFilter filter);
    }
}
