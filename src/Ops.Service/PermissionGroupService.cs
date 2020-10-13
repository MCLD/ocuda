using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Abstract;
using Ocuda.Ops.Models.Definitions;
using Ocuda.Ops.Models.Entities;
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
        private readonly IPermissionGroupPageContentRepository
            _permissionGroupPageContentRepository;
        private readonly IPermissionGroupPodcastItemRepository
            _permissionGroupPodcastItemRepository;
        private readonly IPermissionGroupRepository _permissionGroupRepository;

        public PermissionGroupService(ILogger<PermissionGroupService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            IPermissionGroupApplicationRepository permissionGroupApplicationRepository,
            IPermissionGroupPageContentRepository permissionGroupPageContentRepository,
            IPermissionGroupPodcastItemRepository permissionGroupPodcastItemRepository,
            IPermissionGroupRepository permissionGroupRepository)
            : base(logger, httpContextAccessor)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _permissionGroupApplicationRepository = permissionGroupApplicationRepository
                ?? throw new ArgumentNullException(nameof(permissionGroupApplicationRepository));
            _permissionGroupPageContentRepository
                = permissionGroupPageContentRepository
                ?? throw new ArgumentNullException(nameof(permissionGroupPageContentRepository));
            _permissionGroupPodcastItemRepository
                = permissionGroupPodcastItemRepository
                ?? throw new ArgumentNullException(nameof(permissionGroupPodcastItemRepository));
            _permissionGroupRepository = permissionGroupRepository
                ?? throw new ArgumentNullException(nameof(permissionGroupRepository));
        }

        public async Task<DataWithCount<ICollection<PermissionGroup>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _permissionGroupRepository.GetPaginatedListAsync(filter);
        }

        public async Task<PermissionGroup> AddAsync(PermissionGroup permissionGroup)
        {
            if (permissionGroup == null)
            {
                throw new ArgumentNullException(nameof(permissionGroup));
            }

            permissionGroup.CreatedAt = _dateTimeProvider.Now;
            permissionGroup.CreatedBy = GetCurrentUserId();
            permissionGroup.GroupName = permissionGroup.GroupName?.Trim();
            permissionGroup.PermissionGroupName = permissionGroup.PermissionGroupName?.Trim();

            await ValidateAsync(permissionGroup);

            await _permissionGroupRepository.AddAsync(permissionGroup);
            await _permissionGroupRepository.SaveAsync();

            return permissionGroup;
        }

        public async Task<PermissionGroup> EditAsync(PermissionGroup permissionGroup)
        {
            if (permissionGroup == null)
            {
                throw new ArgumentNullException(nameof(permissionGroup));
            }

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

        public async Task DeleteAsync(int permissionGroupId)
        {
            _permissionGroupRepository.Remove(permissionGroupId);
            await _permissionGroupRepository.SaveAsync();
        }

        private async Task ValidateAsync(PermissionGroup permissionGroup)
        {
            if (await _permissionGroupRepository.IsDuplicateAsync(permissionGroup))
            {
                throw new OcudaException($"Permission group '{permissionGroup.PermissionGroupName}' already exists.");
            }
        }

        public async Task<ICollection<PermissionGroup>> GetAllAsync()
        {
            return await _permissionGroupRepository.GetAllAsync();
        }

        public async Task<ICollection<T>> GetPermissionsAsync<T>(int itemId)
            where T : PermissionGroupMappingBase
        {
            if (typeof(T) == typeof(PermissionGroupPageContent))
            {
                return (ICollection<T>)(await _permissionGroupPageContentRepository
                    .GetByPageHeaderId(itemId));
            }
            else if (typeof(T) == typeof(PermissionGroupPodcastItem))
            {
                return (ICollection<T>)(await _permissionGroupPodcastItemRepository
                    .GetByPodcastId(itemId));
            }

            throw new OcudaException($"Unable to get permissions for {nameof(T)}");
        }

        public async Task AddToPermissionGroupAsync<T>(int itemId, int permissionGroupId)
            where T : PermissionGroupMappingBase
        {
            if (typeof(T) == typeof(PermissionGroupPageContent))
            {
                await _permissionGroupPageContentRepository.AddAsync(new PermissionGroupPageContent
                {
                    PageHeaderId = itemId,
                    PermissionGroupId = permissionGroupId
                });
                await _permissionGroupPageContentRepository.SaveAsync();
                return;
            }
            else if (typeof(T) == typeof(PermissionGroupPodcastItem))
            {
                await _permissionGroupPodcastItemRepository.AddAsync(new PermissionGroupPodcastItem
                {
                    PodcastId = itemId,
                    PermissionGroupId = permissionGroupId
                });
                await _permissionGroupPodcastItemRepository.SaveAsync();
                return;
            }

            throw new OcudaException($"Unable to set permissions for {nameof(T)}");
        }

        public async Task RemoveFromPermissionGroupAsync<T>(int itemId, int permissionGroupId)
            where T : PermissionGroupMappingBase
        {
            if (typeof(T) == typeof(PermissionGroupPageContent))
            {
                _permissionGroupPageContentRepository.Remove(new PermissionGroupPageContent
                {
                    PageHeaderId = itemId,
                    PermissionGroupId = permissionGroupId
                });
                await _permissionGroupPageContentRepository.SaveAsync();
                return;
            }
            else if (typeof(T) == typeof(PermissionGroupPodcastItem))
            {
                _permissionGroupPodcastItemRepository.Remove(new PermissionGroupPodcastItem
                {
                    PodcastId = itemId,
                    PermissionGroupId = permissionGroupId
                });
                await _permissionGroupPodcastItemRepository.SaveAsync();
                return;
            }

            throw new OcudaException($"Unable to remove permissions for {nameof(T)}");
        }

        public async Task<bool>
            HasPermissionAsync<T>(IEnumerable<int> permissionGroupIds)
            where T : PermissionGroupMappingBase
        {
            if (typeof(T) == typeof(PermissionGroupPageContent))
            {
                return await _permissionGroupPageContentRepository
                    .AnyPermissionGroupIdAsync(permissionGroupIds);
            }
            else if (typeof(T) == typeof(PermissionGroupPodcastItem))
            {
                return await _permissionGroupPodcastItemRepository
                    .AnyPermissionGroupIdAsync(permissionGroupIds);
            }

            throw new OcudaException($"Unable to look up permissions for {nameof(T)}");
        }

        public async Task<ICollection<PermissionGroup>>
            GetGroupsAsync(IEnumerable<int> permissionGroupIds)
        {
            return await _permissionGroupRepository.GetGroupsAsync(permissionGroupIds);
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

        public async Task AddApplicationPermissionGroupAsync(string applicationPermission,
            int permissionGroupId)
        {
            var permission = ApplicationPermissionDefinitions.ApplicationPermissions
                .SingleOrDefault(_ => string.Equals(_.Id, applicationPermission,
                    StringComparison.OrdinalIgnoreCase));

            if (permission == null)
            {
                throw new OcudaException("Invalid application permission.");
            }

            await _permissionGroupApplicationRepository.AddAsync(new PermissionGroupApplication
            {
                ApplicationPermission = applicationPermission,
                PermissionGroupId = permissionGroupId
            });
            await _permissionGroupApplicationRepository.SaveAsync();
        }

        public async Task RemoveApplicationPermissionGroupAsync(string applicationPermission,
            int permissionGroupId)
        {
            var permission = ApplicationPermissionDefinitions.ApplicationPermissions
                .SingleOrDefault(_ => string.Equals(_.Id, applicationPermission,
                    StringComparison.OrdinalIgnoreCase));

            if (permission == null)
            {
                throw new OcudaException("Invalid application permission.");
            }

            _permissionGroupApplicationRepository.Remove(new PermissionGroupApplication
            {
                ApplicationPermission = applicationPermission,
                PermissionGroupId = permissionGroupId
            });
            await _permissionGroupPageContentRepository.SaveAsync();
        }
    }
}
