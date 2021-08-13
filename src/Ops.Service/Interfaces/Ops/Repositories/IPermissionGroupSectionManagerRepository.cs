using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPermissionGroupSectionManagerRepository
        : IGenericRepository<PermissionGroupSectionManager>

    {
        public Task AddSaveAsync(int sectionId, int permissionGroupId);

        public Task<bool> AnyPermissionGroupIdAsync(IEnumerable<int> permissionGroupIds);

        public Task<ICollection<PermissionGroupSectionManager>>
            GetByPermissionGroupId(int permissionGroupId);

        public Task<ICollection<PermissionGroupSectionManager>>
            GetBySectionIdAsync(int sectionId);

        public Task<IEnumerable<int>>
            GetByPermissionGroupIdsAsync(IEnumerable<int> permissionGroupIds);

        public Task RemoveSaveAsync(int sectionId, int permissionGroupId);
    }
}