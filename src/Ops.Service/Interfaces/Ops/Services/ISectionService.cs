using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ISectionService
    {
        Task<ICollection<Section>> GetAllAsync();

        Task<Section> GetByIdAsync(int id);

        Task<ICollection<Section>> GetByNamesAsync(ICollection<string> names);

        Task<Section> GetBySlugAsync(string slug);

        Task<int> GetHomeSectionIdAsync();

        Task<ICollection<Section>> GetManagedByCurrentUserAsync();

        Task<bool> IsManagerAsync(int sectionId);
    }
}