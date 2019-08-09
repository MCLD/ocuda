using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IGroupService
    {
        Task<List<Group>> GetAllGroupsAsync();
        Task<List<Group>> GetGroupRegions();
        Task<List<Group>> GetNonGroupRegions();
        Task<List<Group>> GetMissingGroups(List<int> locationGroupIds);
        Task<DataWithCount<ICollection<Group>>> GetPaginatedListAsync(BaseFilter filter);
    }
}
