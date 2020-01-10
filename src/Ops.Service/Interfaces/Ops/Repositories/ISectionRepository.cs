using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ISectionRepository : IOpsRepository<Section, int>
    {
        Task<Section> GetSectionByStubAsync(string stub);
        Task<Section> GetSectionByNameAsync(string name);
        Task<List<Section>> GetAllSectionsAsync();
    }
}
