using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationGroupRepository : IGenericRepository<LocationGroup>
    {
        Task<LocationGroup> GetByIdsAsync(int groupId, int locationId);
        Task<List<LocationGroup>> GetGroupByLocationIdAsync(int locationId);
        Task<List<LocationGroup>> GetLocationsByGroupIdAsync(int groupId);
    }
}

