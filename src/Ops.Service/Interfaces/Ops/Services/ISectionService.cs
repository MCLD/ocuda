using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ISectionService
    {
        Task EnsureDefaultSectionAsync(int sysadminId);
        Task<IEnumerable<SectionWithNavigation>> GetNavigationAsync();
        Task<Section> GetByIdAsync(int id);
        Task<bool> IsValidPathAsync(string path);
        Task<Section> GetByPathAsync(string path);
        Task<DataWithCount<ICollection<Section>>> GetPaginatedListAsync(BaseFilter filter);
        Task<IEnumerable<Section>> GetSectionsAsync();
        Task<int> GetSectionCountAsync();
        Task<Section> CreateAsync(int currentUserId, Section section);
        Task<Section> EditAsync(Section section);
        Task DeleteAsync(int id);
    }
}
