using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ITopicRepository : IGenericRepository<Topic>
    {
        Task<Topic> FindAsync(int id);

        Task<ICollection<Topic>> GetAllAsync();

        Task<DataWithCount<ICollection<Topic>>> GetPaginatedListAsync(BaseFilter filter);
    }
}