using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IGroupService
    {
        Task<Group> AddGroupAsync(Group group);

        Task DeleteAsync(int id);

        Task<Group> EditAsync(Group group);

        Task<List<Group>> GetAllGroupsAsync();

        Task<Group> GetGroupByIdAsync(int groupId);

        Task<Group> GetGroupByStubAsync(string groupType);

        Task<List<Group>> GetGroupRegions();

        Task<ICollection<Group>> GetGroupsByIdsAsync(IEnumerable<int> groupIds);

        Task<List<LocationGroup>> GetLocationGroupsByGroupId(int groupId);

        Task<List<Group>> GetMissingGroups(List<int> locationGroupIds);

        Task<DataWithCount<ICollection<Group>>> GetPaginatedListAsync(BaseFilter filter);

        Task<DataWithCount<ICollection<Group>>> PageItemsAsync(GroupFilter filter);
    }
}