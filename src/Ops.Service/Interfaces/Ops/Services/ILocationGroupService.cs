using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILocationGroupService
    {
        Task<List<LocationGroup>> GetLocationGroupsByLocationAsync(Location location);
        Task<LocationGroup> AddLocationGroupAsync(LocationGroup locationGroup);
        Task DeleteAsync(int groupId, int locationId);
        Task<LocationGroup> GetByIdsAsync(int groupId, int locationId);
        Task<LocationGroup> EditAsync(LocationGroup locationGroup);
    }
}
