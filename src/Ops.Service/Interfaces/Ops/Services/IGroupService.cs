using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IGroupService
    {
        Task<List<Group>> GetAllGroupsAsync();
        Task<List<Group>> GetGroupRegions();
        Task<Group> AddGroupAsync(Group group);
        Task<Group> EditAsync(Group group);
        Task DeleteAsync(int id);
        Task<List<Group>> GetMissingGroups(List<int> locationGroupIds);
        Task<DataWithCount<ICollection<Group>>> GetPaginatedListAsync(BaseFilter filter);
        Task<DataWithCount<ICollection<Group>>> PageItemsAsync(GroupFilter filter);
        Task<Group> GetGroupByIdAsync(int groupId);
        Task<Group> GetGroupByStubAsync(string groupType);
        Task<List<LocationGroup>> GetLocationGroupsByGroupId(int groupId);
    }
}
