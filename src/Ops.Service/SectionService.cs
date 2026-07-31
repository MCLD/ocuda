using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class SectionService(ILogger<SectionService> logger,
        IHttpContextAccessor httpContextAccessor,
        IDateTimeProvider dateTimeProvider,
        IOcudaCache cache,
        IPermissionGroupService permissionGroupService,
        IFileLibraryRepository fileLibraryRepository,
        IPostRepository postRepository,
        ILinkLibraryRepository linkLibraryRepository,
        ISectionRepository sectionRepository,
        IUserService userService)
        : BaseService<SectionService>(logger, httpContextAccessor),
        ISectionService
    {
        public async Task CreateSectionAsync(Section section)
        {
            ArgumentNullException.ThrowIfNull(section);

            var check = await GetBySlugAsync(section.Slug);
            if (check != null)
            {
                throw new OcudaException($"Slug {section.Slug} is already in use");
            }

            section.CreatedAt = dateTimeProvider.Now;
            section.CreatedBy = GetCurrentUserId();

            await sectionRepository.AddAsync(section);
            await sectionRepository.SaveAsync();
        }

        public async Task DeleteSectionAsync(string sectionSlug)
        {
            var section = await GetBySlugAsync(sectionSlug)
                ?? throw new OcudaException($"Unable to find section for slug {sectionSlug}");

            var filter = new BlogFilter(1, 1)
            {
                IncludeDrafts = true,
                SectionId = section.Id,
            };

            var items = new List<string>();

            var fileLibraries = await fileLibraryRepository.GetPaginatedListAsync(filter);
            if (fileLibraries.Count > 0)
            {
                items.Add($"{fileLibraries.Count} file {(fileLibraries.Count == 1 ? "library" : "libraries")}");
            }

            var linkLibraries = await linkLibraryRepository.GetPaginatedListAsync(filter);
            if (linkLibraries.Count > 0)
            {
                items.Add($"{linkLibraries.Count} link {(linkLibraries.Count == 1 ? "library" : "libraries")}");
            }

            var posts = await postRepository.GetPaginatedListAsync(filter);
            if (posts.Count > 0)
            {
                items.Add($"{posts.Count} {(posts.Count == 1 ? "post" : "posts")}");
            }

            if (items.Count > 0)
            {
                throw new OcudaException($"Unable to delete section with slug '{sectionSlug}' becuase it has: {items.HumanCommaList()}");
            }

            var sectionManagerPermissions = await permissionGroupService
                .GetPermissionsAsync<PermissionGroupSectionManager>(section.Id);

            foreach (var permission in sectionManagerPermissions)
            {
                await permissionGroupService
                    .RemoveFromPermissionGroupAsync<PermissionGroupSectionManager>(
                        permission.SectionId,
                        permission.PermissionGroupId);
            }

            sectionRepository.Remove(section.Id);
            await sectionRepository.SaveAsync();
        }

        public async Task<ICollection<Section>> GetAllAsync()
        {
            var sections = await cache
                .GetObjectFromCacheAsync<ICollection<Section>>(Utility.Keys.Cache.OpsSections);

            if (sections == null || sections.Count == 0)
            {
                sections = await sectionRepository.GetAllAsync();
                await cache.SaveToCacheAsync(Utility.Keys.Cache.OpsSections, sections, 1);
            }

            return await userService.IsSupervisor(GetCurrentUserId())
                ? sections
                : [.. sections.Where(_ => !_.SupervisorsOnly)];
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

        public async Task<Section> GetBySlugAsync(string slug)
        {
            var sections = await GetAllAsync();
            return sections.SingleOrDefault(_ => _.Slug.Equals(slug,
                StringComparison.InvariantCultureIgnoreCase));
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
                var authorizedSectionIds = await permissionGroupService
                    .GetItemIdAccessAsync<PermissionGroupSectionManager>(GetPermissionIds());

                sections = sections.Where(_ => authorizedSectionIds.Contains(_.Id)).ToList();
            }

            return sections;
        }
    }
}
