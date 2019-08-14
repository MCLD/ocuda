using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IGroupRepository : IRepository<Group, int>
    {
        Task<List<Group>> GetAllGroupsAsync();
        Task<List<Group>> GetAllGroupRegions();
        Task<List<Group>> GetAllNonGroupRegions();
        Task<bool> IsDuplicateGroupTypeAsync(Group group);
        Task<DataWithCount<ICollection<Group>>> GetPaginatedListAsync(BaseFilter filter);
        Task<ICollection<Group>> PageAsync(GroupFilter filter);
        Task<int> CountAsync(GroupFilter filter);
    }
}
