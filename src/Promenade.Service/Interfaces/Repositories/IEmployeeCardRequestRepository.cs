using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IEmployeeCardRequestRepository : IGenericRepository<EmployeeCardRequest>
    {
        Task AddSaveAsync(EmployeeCardRequest cardRequest);
    }
}
