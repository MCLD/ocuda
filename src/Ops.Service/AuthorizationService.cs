using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Service
{
    public class AuthorizationService
    {
        private readonly ISectionManagerGroupRepository _sectionManagerGroupRepository;
        private readonly ISiteManagerGroupRepository _siteManagerGroupRepository;
        public AuthorizationService(ISectionManagerGroupRepository sectionManagerGroupRepository,
            ISiteManagerGroupRepository siteManagerGroupRepository)
        {
            _sectionManagerGroupRepository = sectionManagerGroupRepository
                ?? throw new ArgumentNullException(nameof(sectionManagerGroupRepository));
            _siteManagerGroupRepository = siteManagerGroupRepository
                ?? throw new ArgumentNullException(nameof(siteManagerGroupRepository));
        }

        public async Task EnsureSiteManagerGroupAsync(int currentUserId, string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new ArgumentNullException(nameof(group));
            }

            var check = await _siteManagerGroupRepository.IsSiteManagerAsync(group.Trim());

            if (!check)
            {
                await _siteManagerGroupRepository.AddAsync(new SiteManagerGroup
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = currentUserId,
                    GroupName = group.Trim()
                });
                await _siteManagerGroupRepository.SaveAsync();
            }
        }

        public async Task<IEnumerable<string>> SiteManagerGroupsAsync()
        {
            var siteManagerGroups = await _siteManagerGroupRepository.ToListAsync(_ => _.GroupName);

            return siteManagerGroups.Select(_ => _.GroupName);
        }

        public async Task<IEnumerable<SectionManagerGroup>> SectionManagerGroupsAsync()
        {
            return await _sectionManagerGroupRepository
                .ToListAsync(_ => _.GroupName);
        }
    }
}
