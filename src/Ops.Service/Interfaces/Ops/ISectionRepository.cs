using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface ISectionRepository : IRepository<Section, int>
    {
        Task<Section> GetDefaultSectionAsync();
        Task<ICollection<Section>> GetNavigationSectionsAsync();
        Task<Section> GetSectionByPathAsync(string path);
    }
}
