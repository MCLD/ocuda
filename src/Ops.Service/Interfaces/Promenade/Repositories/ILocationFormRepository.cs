using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ILocationFormRepository : IGenericRepository<LocationForm>
    {
        Task AddSaveLocationForm(int locationId, int formId);

        Task<LocationForm> FindAsync(int locationId, int formId);
        
        Task RemoveSaveAsync(LocationForm locationForm);
    }
}