using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaRepository : IGenericRepository<Emedia>
    {
        Task<Emedia> FindAsync(int id);
        Task<Emedia> GetIncludingGroupAsync(int id);
        Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListForGroupAsync(int groupId,
            BaseFilter filter);
    }
}
