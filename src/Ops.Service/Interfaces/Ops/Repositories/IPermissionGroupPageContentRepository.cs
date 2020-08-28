using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPermissionGroupPageContentRepository
        : IGenericRepository<PermissionGroupPageContent>
    {
        public Task<ICollection<PermissionGroupPageContent>>
            GetByPermissionGroupId(int permissionGroupId);
        public Task<ICollection<PermissionGroupPageContent>> GetByPageHeaderId(int pageHeaderId);

        public Task<bool> AnyPermissionGroupIdAsync(int[] permissionGroupIds);
    }
}
