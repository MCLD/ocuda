using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IVolunteerUserMappingRepository : IGenericRepository<VolunteerFormUserMapping>
    {
        Task AddSaveFormUserMappingAsync(int formId, int locationId, int userId);

        Task<VolunteerFormUserMapping> FindAsync(int formId, int locationId, int userId);

        Task<ICollection<VolunteerFormUserMapping>> GetByLocationFormAsync(int locationId, int formId);

        Task<ICollection<VolunteerFormUserMapping>> GetByUserAsync(int userId);

        Task RemoveFormUserMappingAsync(int formId, int locationId, int userId);
    }
}