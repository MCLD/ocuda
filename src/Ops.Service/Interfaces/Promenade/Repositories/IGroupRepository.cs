using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IGroupRepository : IGenericRepository<Group>
    {
        Task<int> CountAsync(GroupFilter filter);

        Task<Group> FindAsync(int id);

        Task<List<Group>> GetAllGroupRegions();

        Task<List<Group>> GetAllGroupsAsync();

        Task<ICollection<Group>> GetByIdsAsync(IEnumerable<int> groupIds);

        Task<Group> GetGroupByStubAsync(string groupStub);

        Task<DataWithCount<ICollection<Group>>> GetPaginatedListAsync(BaseFilter filter);

        Task<bool> IsDuplicateGroupTypeAsync(Group group);

        Task<bool> IsDuplicateStubAsync(Group group);

        Task<ICollection<Group>> PageAsync(GroupFilter filter);
    }
}