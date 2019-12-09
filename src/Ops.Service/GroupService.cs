using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class GroupService : BaseService<GroupService>, IGroupService
    {
        private readonly IGroupRepository _groupRepository;

        public GroupService(ILogger<GroupService> logger,
            IHttpContextAccessor httpContextAccessor,
            IGroupRepository groupRepository)
            : base (logger, httpContextAccessor)
        {
            _groupRepository = groupRepository
                ?? throw new ArgumentNullException(nameof(groupRepository));
        }

        public async Task<List<Group>> GetMissingGroups(List<int> locationGroupIds)
        {
            var allGroups = await _groupRepository.GetAllGroupsAsync();
            if (allGroups.Count > 0)
            {
                var groups = new List<Group>();
                foreach (var group in allGroups)
                {
                    if (!locationGroupIds.Contains(group.Id))
                    {
                        groups.Add(group);
                    }
                }
                return groups;
            }
            else
            {
                throw new OcudaException("There are no Groups created");
            }
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            return await _groupRepository.GetAllGroupsAsync();
        }

        public async Task<List<Group>> GetGroupRegions()
        {
            return await _groupRepository.GetAllGroupRegions();
        }

        public async Task<DataWithCount<ICollection<Group>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _groupRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Group> GetGroupByIdAsync(int groupId)
        {
            return await _groupRepository.FindAsync(groupId);
        }

        public async Task<Group> GetGroupByStubAsync(string groupType)
        {
            var group = await _groupRepository.GetGroupByStubAsync(groupType);
            if (group == null)
            {
                throw new OcudaException("Group not found.");
            }
            else
            {
                return group;
            }
        }

        public async Task<Group> AddGroupAsync(Group group)
        {
            group.CreatedAt = DateTime.Now;
            group.CreatedBy = GetCurrentUserId();
            group.GroupType = group.GroupType?.Trim();
            group.Stub = group.Stub.Trim();
            group.SubscriptionUrl = group.SubscriptionUrl?.Trim();
            await ValidateAsync(group);
            await _groupRepository.AddAsync(group);
            await _groupRepository.SaveAsync();
            return group;
        }

        public async Task<Group> EditAsync(Group group)
        {
            var currentGroup = await _groupRepository.FindAsync(group.Id);
            await ValidateAsync(currentGroup);
            currentGroup.GroupType = group.GroupType?.Trim();
            currentGroup.Stub = group.Stub.Trim();
            currentGroup.IsLocationRegion = group.IsLocationRegion;
            currentGroup.SubscriptionUrl = group.SubscriptionUrl?.Trim();
            currentGroup.UpdatedAt = DateTime.Now;
            currentGroup.UpdatedBy = GetCurrentUserId();
            _groupRepository.Update(currentGroup);
            await _groupRepository.SaveAsync();
            return currentGroup;
        }

        public async Task DeleteAsync(int id)
        {
            _groupRepository.Remove(id);
            await _groupRepository.SaveAsync();
        }

        public async Task<DataWithCount<ICollection<Group>>> PageItemsAsync(
            GroupFilter filter)
        {
            return new DataWithCount<ICollection<Group>>
            {
                Data = await _groupRepository.PageAsync(filter),
                Count = await _groupRepository.CountAsync(filter)
            };
        }

        private async Task ValidateAsync(Group group)
        {
            if (await _groupRepository.IsDuplicateGroupTypeAsync(group))
            {
                throw new OcudaException($"Group '{group.GroupType}' already exists.");
            }
            if (await _groupRepository.IsDuplicateStubAsync(group))
            {
                throw new OcudaException($"Group '{group.Stub}' already exists.");
            }
        }
    }
}
