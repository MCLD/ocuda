using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IRosterHeaderRepository : IRepository<RosterHeader, int>
    {
        Task<int?> GetLatestIdAsync();
    }
}
