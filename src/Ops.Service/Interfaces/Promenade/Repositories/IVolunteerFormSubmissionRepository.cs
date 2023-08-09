using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IVolunteerFormSubmissionRepository
        : IGenericRepository<VolunteerFormSubmission>
    {
        Task<ICollection<VolunteerFormSubmission>> GetAllAsync(int locationId, int formId);

        Task<VolunteerFormSubmission> GetByIdAsync(int id);

        Task<CollectionWithCount<VolunteerFormSubmission>> GetPaginatedListAsync(VolunteerSubmissionFilter filter);

        Task<ICollection<VolunteerFormSubmission>> GetPendingNotificationsAsync();

        Task StaffNotifiedAsync(int submissionId);
    }
}