using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IRosterHeaderRepository : IOpsRepository<RosterHeader, int>
    {
        Task<int?> GetLatestIdAsync();
    }
}
