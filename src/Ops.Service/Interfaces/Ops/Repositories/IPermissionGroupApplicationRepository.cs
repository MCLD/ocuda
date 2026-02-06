using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPermissionGroupApplicationRepository
        : IOpsRepository<PermissionGroupApplication, int>
    {
        Task<int> GetApplicationPermissionGroupCountAsync(string permission);

        Task<ICollection<PermissionGroup>> GetApplicationPermissionGroupsAsync(string permission);

        Task<ICollection<string>> GetAssignedPermissions(int permissionGroupId);
    }
}