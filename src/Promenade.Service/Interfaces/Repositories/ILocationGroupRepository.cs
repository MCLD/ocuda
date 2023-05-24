using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationGroupRepository : IGenericRepository<LocationGroup>
    {
        Task<LocationGroup> GetByIdsAsync(int groupId, int locationId);

        Task<ICollection<LocationGroup>> GetGroupByLocationIdAsync(int locationId);

        Task<ICollection<LocationGroup>> GetLocationsByGroupIdAsync(int groupId);
    }
}