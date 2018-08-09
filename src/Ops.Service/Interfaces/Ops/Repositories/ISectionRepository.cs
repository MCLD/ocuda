using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ISectionRepository : IRepository<Section, int>
    {
        Task<Section> GetDefaultSectionAsync();
        Task<ICollection<SectionWithNavigation>> GetNavigationSectionsAsync();
        Task<bool> IsValidPathAsync(string path);
        Task<Section> GetByNameAsync(string name);
        Task<Section> GetByPathAsync(string path);
        Task<DataWithCount<ICollection<Section>>> GetPaginatedListAsync(BaseFilter filter);
        Task<bool> IsDuplicateNameAsync(Section section);
        Task<bool> IsDuplicatePathAsync(Section section);
    }
}
