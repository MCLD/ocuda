﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Abstract;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class PermissionGroupService
        : BaseService<PermissionGroupService>, IPermissionGroupService
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        private readonly IPermissionGroupApplicationRepository
            _permissionGroupApplicationRepository;

        private readonly IPermissionGroupIncidentLocationRepository
            _permissionGroupIncidentLocationRepository;

        private readonly IPermissionGroupPageContentRepository
                    _permissionGroupPageContentRepository;

        private readonly IPermissionGroupPodcastItemRepository
            _permissionGroupPodcastItemRepository;

        private readonly IPermissionGroupProductManagerRepository
            _permissionGroupProductManagerRepository;

        private readonly IPermissionGroupReplaceFilesRepository
                    _permissionGroupReplaceFilesRepository;

        private readonly IPermissionGroupRepository _permissionGroupRepository;

        private readonly IPermissionGroupSectionManagerRepository
            _permissionGroupSectionManagerRepository;

        public PermissionGroupService(ILogger<PermissionGroupService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            IPermissionGroupApplicationRepository permissionGroupApplicationRepository,
            IPermissionGroupIncidentLocationRepository permissionGroupIncidentLocationRepository,
            IPermissionGroupPageContentRepository permissionGroupPageContentRepository,
            IPermissionGroupPodcastItemRepository permissionGroupPodcastItemRepository,
            IPermissionGroupProductManagerRepository permissionGroupProductManagerRepository,
            IPermissionGroupReplaceFilesRepository permissionGroupReplaceFilesRepository,
            IPermissionGroupRepository permissionGroupRepository,
            IPermissionGroupSectionManagerRepository permissionGroupSectionManagerRepository)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(permissionGroupApplicationRepository);
            ArgumentNullException.ThrowIfNull(permissionGroupIncidentLocationRepository);
            ArgumentNullException.ThrowIfNull(permissionGroupPageContentRepository);
            ArgumentNullException.ThrowIfNull(permissionGroupPodcastItemRepository);
            ArgumentNullException.ThrowIfNull(permissionGroupProductManagerRepository);
            ArgumentNullException.ThrowIfNull(permissionGroupReplaceFilesRepository);
            ArgumentNullException.ThrowIfNull(permissionGroupRepository);
            ArgumentNullException.ThrowIfNull(permissionGroupSectionManagerRepository);

            _dateTimeProvider = dateTimeProvider;
            _permissionGroupApplicationRepository = permissionGroupApplicationRepository;
            _permissionGroupIncidentLocationRepository = permissionGroupIncidentLocationRepository;
            _permissionGroupPageContentRepository = permissionGroupPageContentRepository;
            _permissionGroupPodcastItemRepository = permissionGroupPodcastItemRepository;
            _permissionGroupProductManagerRepository = permissionGroupProductManagerRepository;
            _permissionGroupReplaceFilesRepository = permissionGroupReplaceFilesRepository;
            _permissionGroupRepository = permissionGroupRepository;
            _permissionGroupSectionManagerRepository = permissionGroupSectionManagerRepository;
        }

        public async Task AddApplicationPermissionGroupAsync(string applicationPermission,
            int permissionGroupId)
        {
            var permission = ApplicationPermissionDefinitions.ApplicationPermissions
                .SingleOrDefault(_ => string.Equals(_.Id, applicationPermission,
                    StringComparison.OrdinalIgnoreCase))
                ?? throw new OcudaException("Invalid application permission.");

            await _permissionGroupApplicationRepository.AddAsync(new PermissionGroupApplication
            {
                ApplicationPermission = applicationPermission,
                PermissionGroupId = permissionGroupId
            });
            await _permissionGroupApplicationRepository.SaveAsync();
        }

        public async Task<PermissionGroup> AddAsync(PermissionGroup permissionGroup)
        {
            ArgumentNullException.ThrowIfNull(permissionGroup);

            permissionGroup.CreatedAt = _dateTimeProvider.Now;
            permissionGroup.CreatedBy = GetCurrentUserId();
            permissionGroup.GroupName = permissionGroup.GroupName?.Trim();
            permissionGroup.PermissionGroupName = permissionGroup.PermissionGroupName?.Trim();

            await ValidateAsync(permissionGroup);

            await _permissionGroupRepository.AddAsync(permissionGroup);
            await _permissionGroupRepository.SaveAsync();

            return permissionGroup;
        }

        public async Task AddToPermissionGroupAsync<T>(int itemId, int permissionGroupId)
            where T : PermissionGroupMappingBase
        {
            if (GetAddPermissionMap().TryGetValue(typeof(T), out var delegateMethod))
            {
                await delegateMethod(itemId, permissionGroupId);
                _logger.LogInformation("User {CurrentUser} just added permission group id {PermissionGroupId} to {ItemType} id {ItemId}",
                    GetCurrentUserId(),
                    permissionGroupId,
                    typeof(T).Name,
                    itemId);
            }
            else
            {
                throw new OcudaException($"Unable to set permissions for {nameof(T)}");
            }
        }

        public async Task DeleteAsync(int permissionGroupId)
        {
            _permissionGroupRepository.Remove(permissionGroupId);
            await _permissionGroupRepository.SaveAsync();
        }

        public async Task<PermissionGroup> EditAsync(PermissionGroup permissionGroup)
        {
            ArgumentNullException.ThrowIfNull(permissionGroup);

            var currentPermissionGroup
                = await _permissionGroupRepository.FindAsync(permissionGroup.Id);

            currentPermissionGroup.GroupName = permissionGroup.GroupName?.Trim();
            currentPermissionGroup.PermissionGroupName
                = permissionGroup.PermissionGroupName?.Trim();
            currentPermissionGroup.UpdatedAt = _dateTimeProvider.Now;
            currentPermissionGroup.UpdatedBy = GetCurrentUserId();

            await ValidateAsync(currentPermissionGroup);

            _permissionGroupRepository.Update(currentPermissionGroup);
            await _permissionGroupRepository.SaveAsync();

            return currentPermissionGroup;
        }

        public async Task<(bool siteAdminRights, bool contentAdminRights)>
            GetAdminRightsAsync(IEnumerable<int> permissionGroupIds)
        {
            var hasSiteAdminRights = (
                await HasAPermissionAsync<PermissionGroupPageContent>(permissionGroupIds)
                || await HasAPermissionAsync<PermissionGroupPodcastItem>(permissionGroupIds)
                || await HasAPermissionAsync<PermissionGroupProductManager>(permissionGroupIds));

            if (!hasSiteAdminRights)
            {
                var emediaGroups = await GetApplicationPermissionGroupsAsync(ApplicationPermission
                    .EmediaManagement);

                hasSiteAdminRights = emediaGroups
                    .Select(_ => _.Id)
                    .Intersect(permissionGroupIds)
                    .Any();
            }

            var hasContentAdminRights = (
                await HasAPermissionAsync<PermissionGroupReplaceFiles>(permissionGroupIds)
                || await HasAPermissionAsync<PermissionGroupSectionManager>(permissionGroupIds));

            if (!hasContentAdminRights)
            {
                var ddGroups = await GetApplicationPermissionGroupsAsync(ApplicationPermission
                    .DigitalDisplayContentManagement);

                hasContentAdminRights = ddGroups
                    .Select(_ => _.Id)
                    .Intersect(permissionGroupIds)
                    .Any();
            }

            return (siteAdminRights: hasSiteAdminRights, contentAdminRights: hasContentAdminRights);
        }

        public async Task<ICollection<PermissionGroup>> GetAllAsync()
        {
            var permissions = await _permissionGroupRepository.GetAllAsync();
            return permissions.OrderBy(_ => _.PermissionGroupName).ToList();
        }

        public async Task<int> GetApplicationPermissionGroupCountAsync(string permission)
        {
            return await _permissionGroupApplicationRepository
                .GetApplicationPermissionGroupCountAsync(permission);
        }

        public async Task<ICollection<PermissionGroup>> GetApplicationPermissionGroupsAsync(
            string permission)
        {
            return await _permissionGroupApplicationRepository
                .GetApplicationPermissionGroupsAsync(permission);
        }

        public async Task<ICollection<PermissionGroup>>
            GetGroupsAsync(IEnumerable<int> permissionGroupIds)
        {
            return await _permissionGroupRepository.GetGroupsAsync(permissionGroupIds);
        }

        public async Task<IEnumerable<int>>
            GetItemIdAccessAsync<T>(IEnumerable<int> permissionGroupIds)
            where T : PermissionGroupMappingBase
        {
            if (GetItemAccessMap().TryGetValue(typeof(T), out var delegateMethod))
            {
                return await delegateMethod(permissionGroupIds.ToList());
            }
            else
            {
                throw new OcudaException($"Unable to look up permissions for {nameof(T)}");
            }
        }

        public async Task<DataWithCount<ICollection<PermissionGroup>>>
            GetPaginatedListAsync(BaseFilter filter)
        {
            return await _permissionGroupRepository.GetPaginatedListAsync(filter);
        }

        public async Task<ICollection<T>> GetPermissionsAsync<T>(int itemId)
            where T : PermissionGroupMappingBase
        {
            if (GetLookupPermissionMap().TryGetValue(typeof(T), out var delegateMethod))
            {
                return (await delegateMethod(itemId)) as ICollection<T>;
            }

            throw new OcudaException($"Unable to get permissions for {nameof(T)}");
        }

        public async Task<bool>
            HasAPermissionAsync<T>(IEnumerable<int> permissionGroupIds)
            where T : PermissionGroupMappingBase
        {
            if (GetHasPermissionMap().TryGetValue(typeof(T), out var delegateMethod))
            {
                return await delegateMethod(permissionGroupIds);
            }
            else
            {
                throw new OcudaException($"Unable to look up permissions for {nameof(T)}");
            }
        }

        public async Task RemoveApplicationPermissionGroupAsync(string applicationPermission,
            int permissionGroupId)
        {
            var permission = ApplicationPermissionDefinitions.ApplicationPermissions
                .SingleOrDefault(_ => string.Equals(_.Id, applicationPermission,
                    StringComparison.OrdinalIgnoreCase))
                ?? throw new OcudaException("Invalid application permission.");
            
            _permissionGroupApplicationRepository.Remove(new PermissionGroupApplication
            {
                ApplicationPermission = applicationPermission,
                PermissionGroupId = permissionGroupId
            });
            await _permissionGroupPageContentRepository.SaveAsync();
        }

        public async Task RemoveFromPermissionGroupAsync<T>(int itemId, int permissionGroupId)
            where T : PermissionGroupMappingBase
        {
            if (GetRemovePermissionMap().TryGetValue(typeof(T), out var delegateMethod))
            {
                await delegateMethod(itemId, permissionGroupId);
                _logger.LogInformation("User {CurrentUser} just added permission group id {PermissionGroupId} to {ItemType} id {ItemId}",
                    GetCurrentUserId(),
                    permissionGroupId,
                    typeof(T).Name,
                    itemId);
            }
            else
            {
                throw new OcudaException($"Unable to remove permissions for {nameof(T)}");
            }
        }

        private Dictionary<Type, Func<int, int, Task>> GetAddPermissionMap()
        {
            return new Dictionary<Type, Func<int, int, Task>>
            {
                { typeof(PermissionGroupIncidentLocation), async(_, __)
                    => await _permissionGroupIncidentLocationRepository.AddSaveAsync(_, __) },
                { typeof(PermissionGroupPageContent), async (_, __)
                    => await _permissionGroupPageContentRepository.AddSaveAsync(_, __) },
                { typeof(PermissionGroupPodcastItem), async (_, __)
                    => await _permissionGroupPodcastItemRepository.AddSaveAsync(_, __) },
                { typeof(PermissionGroupProductManager), async(_, __)
                    => await _permissionGroupProductManagerRepository.AddSaveAsync(_, __) },
                { typeof(PermissionGroupReplaceFiles), async(_, __)
                    => await _permissionGroupReplaceFilesRepository.AddSaveAsync(_, __) },
                { typeof(PermissionGroupSectionManager), async(_, __)
                    => await _permissionGroupSectionManagerRepository.AddSaveAsync(_, __) }
            };
        }

        private Dictionary<Type, Func<IEnumerable<int>, Task<bool>>> GetHasPermissionMap()
        {
            return new Dictionary<Type, Func<IEnumerable<int>, Task<bool>>>
            {
                { typeof(PermissionGroupIncidentLocation), async _
                    => await _permissionGroupIncidentLocationRepository.AnyPermissionGroupIdAsync(_) },
                { typeof(PermissionGroupPodcastItem), async _
                    => await _permissionGroupPodcastItemRepository.AnyPermissionGroupIdAsync(_) },
                { typeof(PermissionGroupPageContent), async _
                    => await _permissionGroupPageContentRepository.AnyPermissionGroupIdAsync(_) },
                { typeof(PermissionGroupReplaceFiles), async _
                    => await _permissionGroupReplaceFilesRepository.AnyPermissionGroupIdAsync(_) },
                { typeof(PermissionGroupProductManager), async _
                    => await _permissionGroupProductManagerRepository.AnyPermissionGroupIdAsync(_) },
                { typeof(PermissionGroupSectionManager), async _
                    => await _permissionGroupSectionManagerRepository.AnyPermissionGroupIdAsync(_) }
            };
        }

        private Dictionary<Type, Func<IEnumerable<int>, Task<IEnumerable<int>>>> GetItemAccessMap()
        {
            return new Dictionary<Type, Func<IEnumerable<int>, Task<IEnumerable<int>>>>
            {
                { typeof(PermissionGroupIncidentLocation), async _
                    => await _permissionGroupIncidentLocationRepository.GetByPermissionGroupIdsAsync(_) },
                { typeof(PermissionGroupPodcastItem), async _
                    => await _permissionGroupPodcastItemRepository.GetByPermissionGroupIdsAsync(_) },
                { typeof(PermissionGroupPageContent), async _
                    => await _permissionGroupPageContentRepository.GetByPermissionGroupIdsAsync(_) },
                { typeof(PermissionGroupReplaceFiles), async _
                    => await _permissionGroupReplaceFilesRepository.GetByPermissionGroupIdsAsync(_) },
                { typeof(PermissionGroupProductManager), async _
                    => await _permissionGroupProductManagerRepository.GetByPermissionGroupIdsAsync(_) },
                { typeof(PermissionGroupSectionManager), async _
                    => await _permissionGroupSectionManagerRepository.GetByPermissionGroupIdsAsync(_) }
            };
        }

        private Dictionary<Type, Func<int, Task<object>>> GetLookupPermissionMap()
        {
            return new Dictionary<Type, Func<int, Task<object>>>
            {
                { typeof(PermissionGroupIncidentLocation), async _
                    => await _permissionGroupIncidentLocationRepository.GetByLocationIdAsync(_) },
                { typeof(PermissionGroupPodcastItem), async _
                    => await _permissionGroupPodcastItemRepository.GetByPodcastId(_) },
                { typeof(PermissionGroupPageContent), async _
                    => await _permissionGroupPageContentRepository.GetByPageHeaderId(_) },
                { typeof(PermissionGroupReplaceFiles), async _
                    => await _permissionGroupReplaceFilesRepository.GetByFileLibraryId(_) },
                { typeof(PermissionGroupProductManager), async _
                    => await _permissionGroupProductManagerRepository.GetByProductIdAsync(_) },
                { typeof(PermissionGroupSectionManager), async _
                    => await _permissionGroupSectionManagerRepository.GetBySectionIdAsync(_) }
            };
        }

        private Dictionary<Type, Func<int, int, Task>> GetRemovePermissionMap()
        {
            return new Dictionary<Type, Func<int, int, Task>>
            {
                { typeof(PermissionGroupIncidentLocation),async (_, __)
                    => await _permissionGroupIncidentLocationRepository.RemoveSaveAsync(_, __) },
                { typeof(PermissionGroupPodcastItem),async (_, __)
                    => await _permissionGroupPodcastItemRepository.RemoveSaveAsync(_, __) },
                { typeof(PermissionGroupPageContent),async (_, __)
                    => await _permissionGroupPageContentRepository.RemoveSaveAsync(_, __) },
                { typeof(PermissionGroupReplaceFiles), async(_, __)
                    => await _permissionGroupReplaceFilesRepository.RemoveSaveAsync(_, __) },
                { typeof(PermissionGroupProductManager), async(_, __)
                    => await _permissionGroupProductManagerRepository.RemoveSaveAsync(_, __) },
                { typeof(PermissionGroupSectionManager), async(_, __)
                    => await _permissionGroupSectionManagerRepository.RemoveSaveAsync(_, __) }
            };
        }

        private async Task ValidateAsync(PermissionGroup permissionGroup)
        {
            if (await _permissionGroupRepository.IsDuplicateAsync(permissionGroup))
            {
                throw new OcudaException($"Permission group '{permissionGroup.PermissionGroupName}' already exists.");
            }
        }
    }
}