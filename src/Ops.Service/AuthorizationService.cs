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
        private readonly IPermissionGroupRepository _permissionGroupRepository;
        private readonly ISectionManagerGroupRepository _sectionManagerGroupRepository;

        public AuthorizationService(IClaimGroupRepository claimGroupRepository,
            IPermissionGroupRepository permissionGroupRepository,
            ISectionManagerGroupRepository sectionManagerGroupRepository)
        {
            _claimGroupRepository = claimGroupRepository
                ?? throw new ArgumentNullException(nameof(claimGroupRepository));
            _permissionGroupRepository = permissionGroupRepository
                ?? throw new ArgumentNullException(nameof(permissionGroupRepository));
            _sectionManagerGroupRepository = sectionManagerGroupRepository
                ?? throw new ArgumentNullException(nameof(sectionManagerGroupRepository));
        }

        public async Task EnsureSiteManagerGroupAsync(int currentUserId, string group)
        {
            if (string.IsNullOrEmpty(group))
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

        public async Task<IEnumerable<SectionManagerGroup>> GetSectionManagerGroupsAsync()
        {
            return await _sectionManagerGroupRepository.ToListAsync(_ => _.GroupName);
        }

        public async Task<ICollection<PermissionGroup>> GetPermissionGroupsAsync()
        {
            return await _permissionGroupRepository.ToListAsync(_ => _.GroupName);
        }
    }
}
