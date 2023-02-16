using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IVolunteerFormSubmissionRepository : IGenericRepository<VolunteerFormSubmission>
    {
        Task AddAsync(VolunteerFormSubmission submission);

        Task SaveAsync();
    }
}