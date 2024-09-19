using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPermissionGroupIncidentLocationRepository
        : IGenericRepository<PermissionGroupIncidentLocation>
    {
        public Task AddSaveAsync(int locationId, int permissionGroupId);

        public Task<bool> AnyPermissionGroupIdAsync(IEnumerable<int> permissionGroupIds);

        public Task<ICollection<PermissionGroupIncidentLocation>>
            GetByLocationIdAsync(int locationId);

        public Task<IEnumerable<int>>
            GetByPermissionGroupIdsAsync(IEnumerable<int> permissionGroupIds);

        public Task RemoveSaveAsync(int locationId, int permissionGroupId);
    }
}