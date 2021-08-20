using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IAuthorizationService
    {
        Task EnsureSiteManagerGroupAsync(int currentUserId, string group);

        Task<ICollection<string>> GetAdminClaimsAsync(IEnumerable<int> permissionGroupIds);

        Task<ICollection<ClaimGroup>> GetClaimGroupsAsync();

        Task<ICollection<PermissionGroup>> GetPermissionGroupsAsync();
    }
}