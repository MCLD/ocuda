using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPermissionGroupPageContentRepository
        : IGenericRepository<PermissionGroupPageContent>
    {
        public Task AddSaveAsync(int pageHeaderId, int permissionGroupId);

        public Task<bool> AnyPermissionGroupIdAsync(IEnumerable<int> permissionGroupIds);

        public Task<ICollection<PermissionGroupPageContent>> GetByPageHeaderId(int pageHeaderId);

        public Task<ICollection<PermissionGroupPageContent>>
            GetByPermissionGroupId(int permissionGroupId);

        public Task<IEnumerable<int>>
            GetByPermissionGroupIdsAsync(IEnumerable<int> permissionGroupIds);

        public Task RemoveSaveAsync(int pageHeaderId, int permissionGroupId);
    }
}