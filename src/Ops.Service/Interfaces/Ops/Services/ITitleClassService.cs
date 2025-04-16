using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ITitleClassService
    {
        Task AddTitleAsync(int titleClassId, string title);

        Task<TitleClass> GetAsync(int titleClassId);

        Task<CollectionWithCount<TitleClass>> GetPaginatedAsync(BaseFilter filter);

        Task<IEnumerable<TitleClass>> GetTitleClassByTitleAsync(string title);

        Task<int> NewTitleClassificationAsync(string titleClassName, string title);

        Task<bool> RemoveTitleAsync(int titleClassId, string title);
    }
}