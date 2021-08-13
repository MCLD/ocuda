using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class SectionService : BaseService<SectionService>, ISectionService
    {
        private readonly IOcudaCache _cache;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ISectionRepository _sectionRepository;
        private readonly IUserService _userService;

        public SectionService(ILogger<SectionService> logger,
            IHttpContextAccessor httpContextAccessor,
            IOcudaCache cache,
            IPermissionGroupService permissionGroupService,
            ISectionRepository sectionRepository,
            IUserService userService) : base(logger, httpContextAccessor)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<ICollection<Section>> GetAllAsync()
        {
            var sections = await _cache
                .GetObjectFromCacheAsync<ICollection<Section>>(Utility.Keys.Cache.OpsSections);

            if (sections == null || sections.Count == 0)
            {
                sections = await _sectionRepository.GetAllAsync();
                await _cache.SaveToCacheAsync(Utility.Keys.Cache.OpsSections, sections, 1);
            }

            var isSupervisor = await _userService.IsSupervisor(GetCurrentUserId());

            return isSupervisor
                ? sections
                : sections.Where(_ => !_.SupervisorsOnly).ToList();
        }

        public async Task<Section> GetByIdAsync(int id)
        {
            var sections = await GetAllAsync();
            return sections.SingleOrDefault(_ => _.Id == id);
        }

        public async Task<ICollection<Section>> GetByNamesAsync(ICollection<string> names)
        {
            var sections = await GetAllAsync();
            return sections.Where(_ => names.Contains(_.Name)).ToList();
        }

        public async Task<Section> GetByStubAsync(string stub)
        {
            var sections = await GetAllAsync();
            return sections.SingleOrDefault(_ => _.Stub == stub);
        }

        public async Task<int> GetHomeSectionIdAsync()
        {
            var sections = await GetAllAsync();
            return sections.Where(_ => _.IsHomeSection).Select(_ => _.Id).Single();
        }

        public async Task<ICollection<Section>> GetManagedByCurrentUserAsync()
        {
            var sections = await GetAllAsync();

            if (!IsSiteManager())
            {
                var authorizedSectionIds = await _permissionGroupService
                    .GetItemIdAccessAsync<PermissionGroupSectionManager>(GetPermissionIds());

                sections = sections.Where(_ => authorizedSectionIds.Contains(_.Id)).ToList();
            }

            return sections;
        }

        public async Task<bool> IsManagerAsync(int sectionId)
        {
            var access = IsSiteManager();

            if (!access)
            {
                var permissionGroups = await _permissionGroupService
                    .GetPermissionsAsync<PermissionGroupSectionManager>(sectionId);

                access = permissionGroups.Any(_ => _.SectionId == sectionId);
            }

            if(!access)
            {
                return false;
            }

            // check supervisory access
            var section = await GetByIdAsync(sectionId);

            return section != null;
        }
    }
}