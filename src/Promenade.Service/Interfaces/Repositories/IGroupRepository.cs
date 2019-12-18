using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IGroupRepository : IGenericRepository<Group, int>
    {
        Task<List<Group>> GetAllGroups();
    }
}

