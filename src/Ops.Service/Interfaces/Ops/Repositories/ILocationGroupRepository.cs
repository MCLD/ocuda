using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ILocationGroupRepository : IRepository<LocationGroup, int>
    {
        Task<List<LocationGroup>> GetLocationGroupsByLocationAsync(Location location);
        Task<bool> IsDuplicateAsync(LocationGroup locationGroup);
    }
}
