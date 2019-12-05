using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ISectionService
    {
        Task<Section> GetSectionByStubAsync(string stub);
        Task<List<Section>> GetSectionsByNamesAsync(List<string> names);
        Task<List<Section>> GetAllSectionsAsync();
        Task<Section> GetByIdAsync(int id);
    }
}
