using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IGroupRepository : IGenericRepository<Group>
    {
        Task<Group> FindAsync(int id);
        Task<List<Group>> GetAllGroups();
    }
}

