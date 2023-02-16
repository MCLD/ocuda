using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IVolunteerUserMappingRepository : IGenericRepository<VolunteerFormUserMapping>
    {
        Task AddSaveFormUserMappingAsync(int formId, int locationId, int userId);

        Task<List<VolunteerFormUserMapping>> FindAsync(int locationId, int formId);

        Task<VolunteerFormUserMapping> FindAsync(int formId, int locationId, int userId);

        Task RemoveFormUserMappingAsync(int formId, int locationId, int userId);
    }
}