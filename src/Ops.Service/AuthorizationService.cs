using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Service
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IClaimGroupRepository _claimGroupRepository;
        private readonly IPermissionGroupService _permissionGroupService;

        public AuthorizationService(IClaimGroupRepository claimGroupRepository,
            IPermissionGroupService permissionGroupService)
        {
            _claimGroupRepository = claimGroupRepository
                ?? throw new ArgumentNullException(nameof(claimGroupRepository));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        public async Task EnsureSiteManagerGroupAsync(int currentUserId, string group)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group));
            }

            var check = await _claimGroupRepository.IsClaimGroup(ClaimType.SiteManager,
                group.Trim());

            if (!check)
            {
                await _claimGroupRepository.AddAsync(new ClaimGroup
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = currentUserId,
                    GroupName = group.Trim(),
                    ClaimType = ClaimType.SiteManager
                });
                await _claimGroupRepository.SaveAsync();
            }
        }

        public async Task<ICollection<ClaimGroup>> GetClaimGroupsAsync()
        {
            return await _claimGroupRepository.ToListAsync(_ => _.GroupName);
        }

        public async Task<ICollection<PermissionGroup>> GetPermissionGroupsAsync()
        {
            return await _permissionGroupService.GetAllAsync();
        }

        public async Task<ICollection<string>> GetAdminClaimsAsync(IEnumerable<int> permissionGroupIds)
        {
            var claims = new List<string>();

            (bool siteAdminRights, bool contentAdminRights)
                = await _permissionGroupService.GetAdminRightsAsync(permissionGroupIds);

            if (siteAdminRights)
            {
                claims.Add(ClaimType.HasSiteAdminRights);
            }

            if (contentAdminRights)
            {
                claims.Add(ClaimType.HasContentAdminRights);
            }

            return claims;
        }
    }
}