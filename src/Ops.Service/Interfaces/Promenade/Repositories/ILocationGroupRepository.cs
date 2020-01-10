using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ILocationGroupRepository : IGenericRepository<LocationGroup>
    {
        Task<LocationGroup> GetByIdsAsync(int groupId, int locationId);
        Task<List<LocationGroup>> GetLocationGroupsByLocationAsync(Location location);
        Task<bool> IsDuplicateAsync(LocationGroup locationGroup);
    }
}
