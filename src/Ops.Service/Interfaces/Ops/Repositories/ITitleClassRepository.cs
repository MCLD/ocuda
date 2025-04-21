using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ITitleClassRepository : IOpsRepository<TitleClass, int>
    {
        Task AddTitleAsync(int userId, int titleClassId, string title);

        Task<IEnumerable<TitleClass>> GetByTitleAsync(string title);

        Task<CollectionWithCount<TitleClass>> GetPaginatedAsync(BaseFilter filter);

        Task<int> NewTitleClassificationAsync(int userId, string titleClassName, string title);

        Task<bool> RemoveTitleAsync(int titleClassId, string title);
    }
}
