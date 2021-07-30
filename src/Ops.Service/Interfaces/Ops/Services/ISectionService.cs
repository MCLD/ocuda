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

        Task<Section> GetByStubAsync(string stub);
    }
}