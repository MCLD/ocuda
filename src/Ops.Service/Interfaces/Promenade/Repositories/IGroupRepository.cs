using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IGroupRepository : IGenericRepository<Group>
    {
        Task<Group> FindAsync(int id);
        Task<List<Group>> GetAllGroupsAsync();
        Task<List<Group>> GetAllGroupRegions();
        Task<bool> IsDuplicateGroupTypeAsync(Group group);
        Task<bool> IsDuplicateStubAsync(Group group);
        Task<DataWithCount<ICollection<Group>>> GetPaginatedListAsync(BaseFilter filter);
        Task<ICollection<Group>> PageAsync(GroupFilter filter);
        Task<int> CountAsync(GroupFilter filter);
        Task<Group> GetGroupByStubAsync(string groupStub);
    }
}
