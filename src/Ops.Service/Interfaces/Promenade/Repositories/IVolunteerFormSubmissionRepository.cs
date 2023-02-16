using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IVolunteerFormSubmissionRepository : IGenericRepository<VolunteerFormSubmission>
    {
        Task<ICollection<VolunteerFormSubmission>> GetAllAsync(int locationId, int formId);

        Task<VolunteerFormSubmission> GetByIdAsync(int id);
    }
}