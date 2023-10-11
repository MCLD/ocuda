using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IVolunteerFormRepository : IGenericRepository<VolunteerForm>
    {
        Task AddSaveAsync(VolunteerForm form);

        Task<ICollection<VolunteerForm>> FindAllAsync();

        Task<VolunteerForm> FindAsync(int id);

        Task<ICollection<VolunteerForm>> FindBySegmentIdAsync(int segmentId);

        Task<VolunteerForm> FindByTypeAsync(VolunteerFormType type);

        Task<VolunteerForm> FindByTypeAsync(int typeId);

        Task<IDictionary<VolunteerFormType, int>> GetEmailSetupMappingAsync();

        Task<DataWithCount<ICollection<VolunteerForm>>> GetPaginatedListAsync(BaseFilter filter);

        Task UpdateSaveAsync(VolunteerForm form);
    }
}