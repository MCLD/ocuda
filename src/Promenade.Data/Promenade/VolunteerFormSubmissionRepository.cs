using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class VolunteerFormSubmissionRepository : GenericRepository<PromenadeContext, VolunteerFormSubmission>, IVolunteerFormSubmissionRepository
    {
        public VolunteerFormSubmissionRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<VolunteerFormSubmissionRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddAsync(VolunteerFormSubmission submission)
        {
            await DbSet.AddAsync(submission);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}