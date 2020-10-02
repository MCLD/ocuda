using System.Collections.Generic;
using System.Threading.Tasks;
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
        Task<ICollection<PermissionGroupPageContent>> GetPagePermissionsAsync(int pageHeaderId);
        Task AddPageHeaderPermissionGroupAsync(int pageHeaderId, int permissionGroupId);
        Task RemovePageHeaderPermissionGroupAsync(int pageHeaderId, int permissionGroupId);
        Task<bool> HasPageContentPermissionAsync(int[] permissionGroupIds);
        Task<ICollection<PermissionGroup>> GetGroupsAsync(int[] permissionGroupIds);
        Task<int> GetApplicationPermissionGroupCountAsync(string permission);
        Task<ICollection<PermissionGroup>> GetApplicationPermissionGroupsAsync(string permission);
        Task AddApplicationPermissionGroupAsync(string applicationPermission, int permissionGroupId);
        Task RemoveApplicationPermissionGroupAsync(string applicationPermission, int permissionGroupId);
    }
}
