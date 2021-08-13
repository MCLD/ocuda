using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPermissionGroupReplaceFilesRepository
        : IGenericRepository<PermissionGroupReplaceFiles>
    {
        public Task AddSaveAsync(int fileLibraryId, int permissionGroupId);

        public Task<bool> AnyPermissionGroupIdAsync(IEnumerable<int> permissionGroupIds);

        public Task<ICollection<PermissionGroupReplaceFiles>>
            GetByFileLibraryId(int fileLibraryId);

        public Task<ICollection<PermissionGroupReplaceFiles>>
            GetByPermissionGroupIdAsync(int permissionGroupId);

        public Task<IEnumerable<int>>
            GetByPermissionGroupIdsAsync(IEnumerable<int> permissionGroupIds);

        public Task RemoveSaveAsync(int fileLibraryId, int permissionGroupId);
    }
}