using System;
using System.Collections.Generic;
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
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class PermissionGroupService
        : BaseService<PermissionGroupService>, IPermissionGroupService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPermissionGroupPageContentRepository
            _permissionGroupPageContentRepository;
        private readonly IPermissionGroupRepository _permissionGroupRepository;

        public PermissionGroupService(ILogger<PermissionGroupService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            IPermissionGroupPageContentRepository permissionGroupPageContentRepository,
            IPermissionGroupRepository permissionGroupRepository)
            : base(logger, httpContextAccessor)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _permissionGroupPageContentRepository
                = permissionGroupPageContentRepository
                ?? throw new ArgumentNullException(nameof(permissionGroupPageContentRepository));
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

        public async Task<ICollection<PermissionGroupPageContent>>
            GetPagePermissionsAsync(int pageHeaderId)
        {
            return await _permissionGroupPageContentRepository.GetByPageHeaderId(pageHeaderId);
        }

        public async Task AddPageHeaderPermissionGroupAsync(int pageHeaderId, int permissionGroupId)
        {
            await _permissionGroupPageContentRepository.AddAsync(new PermissionGroupPageContent
            {
                PageHeaderId = pageHeaderId,
                PermissionGroupId = permissionGroupId
            });
            await _permissionGroupPageContentRepository.SaveAsync();
        }
        public async Task RemovePageHeaderPermissionGroupAsync(int pageHeaderId, int permissionGroupId)
        {
            _permissionGroupPageContentRepository.Remove(new PermissionGroupPageContent
            {
                PageHeaderId = pageHeaderId,
                PermissionGroupId = permissionGroupId
            });
            await _permissionGroupPageContentRepository.SaveAsync();
        }

        public async Task<bool> HasPageContentPermissionAsync(int[] permissionGroupIds)
        {
            return await _permissionGroupPageContentRepository
                .AnyPermissionGroupIdAsync(permissionGroupIds);
        }

        public async Task<ICollection<PermissionGroup>> GetGroupsAsync(int[] permissionGroupIds)
        {
            return await _permissionGroupRepository.GetGroupsAsync(permissionGroupIds);
        }
    }
}
