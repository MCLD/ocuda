using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IEmployeeCardResultRepository : IOpsRepository<EmployeeCardResult, int>
    {
        Task<CollectionWithCount<EmployeeCardResult>> GetPaginatedAsync(BaseFilter filter);
    }
}
