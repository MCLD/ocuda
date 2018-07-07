using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Service
{
    public class AuthorizationService
    {
        private readonly ISectionManagerGroupRepository _sectionManagerGroupRepository;
        private readonly IClaimGroupRepository _claimGroupRepository;
        public AuthorizationService(ISectionManagerGroupRepository sectionManagerGroupRepository,
            IClaimGroupRepository claimGroupRepository)
        {
            _sectionManagerGroupRepository = sectionManagerGroupRepository
                ?? throw new ArgumentNullException(nameof(sectionManagerGroupRepository));
            _claimGroupRepository = claimGroupRepository
                ?? throw new ArgumentNullException(nameof(claimGroupRepository));
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
            return await _sectionManagerGroupRepository
                .ToListAsync(_ => _.GroupName);
        }
    }
}
