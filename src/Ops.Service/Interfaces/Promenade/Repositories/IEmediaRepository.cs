using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaRepository : IGenericRepository<Emedia>
    {
        Task ApplySlugAsync(int id, string slug);

        Task<Emedia> FindAsync(string name, string link);

        Task<Emedia> FindAsync(int id);

        Task<Emedia> GetIncludingGroupAsync(int id);

        Task<IDictionary<int, string>> GetMissingSlugsAsync();

        Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListForGroupAsync(int groupId,
                    BaseFilter filter);

        Task<string> GetUnusedSlugAsync(string slug);
    }
}