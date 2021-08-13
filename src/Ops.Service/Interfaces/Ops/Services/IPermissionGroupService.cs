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
        Task AddApplicationPermissionGroupAsync(string applicationPermission,
            int permissionGroupId);

        Task<PermissionGroup> AddAsync(PermissionGroup permissionGroup);

        Task AddToPermissionGroupAsync<T>(int itemId, int permissionGroupId)
            where T : PermissionGroupMappingBase;

        Task DeleteAsync(int permissionGroupId);

        Task<PermissionGroup> EditAsync(PermissionGroup permissionGroup);

        Task<ICollection<PermissionGroup>> GetAllAsync();

        Task<int> GetApplicationPermissionGroupCountAsync(string permission);

        Task<ICollection<PermissionGroup>> GetApplicationPermissionGroupsAsync(string permission);

        Task<ICollection<PermissionGroup>> GetGroupsAsync(IEnumerable<int> permissionGroupIds);

        /// <summary>
        /// Get item ids of type T for the provided set of permissionGroupIds.
        /// </summary>
        /// <typeparam name="T">The <see cref="PermissionGroupMappingBase"/> representing what
        /// type of permission you seek.</typeparam>
        /// <param name="permissionGroupIds">A list of permissionGroupIds</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of item ids.</returns>
        Task<IEnumerable<int>> GetItemIdAccessAsync<T>(IEnumerable<int> permissionGroupIds)
            where T : PermissionGroupMappingBase;

        Task<DataWithCount<ICollection<PermissionGroup>>>
            GetPaginatedListAsync(BaseFilter filter);

        /// <summary>
        /// Get the appropriate <see cref="PermissionGroupMappingBase"/> based on an item id.
        /// </summary>
        /// <typeparam name="T">The <see cref="PermissionGroupMappingBase"/> representing what
        /// type of permission you seek.</typeparam>
        /// <param name="itemId">The item id</param>
        /// <returns>An <see cref="ICollection{T}"/> of <see cref="PermissionGroupMappingBase"/>
        /// objects.</returns>
        Task<ICollection<T>> GetPermissionsAsync<T>(int itemId)
            where T : PermissionGroupMappingBase;

        /// <summary>
        /// See if the provided list of permissionGroupIds has any permissions of type
        /// <see cref="PermissionGroupMappingBase"/> - mostly to determine if the user should have
        /// access to the section at all.
        /// </summary>
        /// <typeparam name="T">The <see cref="PermissionGroupMappingBase"/> representing what
        /// type of permission you seek.</typeparam>
        /// <param name="permissionGroupIds">A list of permissionGroupIds</param>
        /// <returns>Whether or not there's one or more permissions.</returns>
        Task<bool> HasAPermissionAsync<T>(IEnumerable<int> permissionGroupIds)
            where T : PermissionGroupMappingBase;

        Task RemoveApplicationPermissionGroupAsync(string applicationPermission,
                            int permissionGroupId);

        Task RemoveFromPermissionGroupAsync<T>(int itemId, int permissionGroupId)
            where T : PermissionGroupMappingBase;
    }
}