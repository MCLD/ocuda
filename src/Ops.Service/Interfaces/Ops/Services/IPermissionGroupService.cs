using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Abstract;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IPermissionGroupService
    {
        Task<DataWithCount<ICollection<PermissionGroup>>> GetPaginatedListAsync(BaseFilter filter);
        Task<PermissionGroup> AddAsync(PermissionGroup permissionGroup);
        Task<PermissionGroup> EditAsync(PermissionGroup permissionGroup);
        Task DeleteAsync(int permissionGroupId);
        Task<ICollection<PermissionGroup>> GetAllAsync();
        Task<int> GetApplicationPermissionGroupCountAsync(string permission);
        Task<ICollection<PermissionGroup>> GetApplicationPermissionGroupsAsync(string permission);
        Task AddApplicationPermissionGroupAsync(string applicationPermission, int permissionGroupId);
        Task RemoveApplicationPermissionGroupAsync(string applicationPermission, int permissionGroupId);
        Task<ICollection<T>> GetPermissionsAsync<T>(int itemId)
            where T : PermissionGroupMappingBase;
        Task AddToPermissionGroupAsync<T>(int itemId, int permissionGroupId)
            where T : PermissionGroupMappingBase;
        Task RemoveFromPermissionGroupAsync<T>(int itemId, int permissionGroupId)
            where T : PermissionGroupMappingBase;
        Task<bool> HasPermissionAsync<T>(IEnumerable<int> permissionGroupIds)
            where T : PermissionGroupMappingBase;
        Task<ICollection<PermissionGroup>> GetGroupsAsync(IEnumerable<int> permissionGroupIds);
    }
}
