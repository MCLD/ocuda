using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Promenade.Data.Promenade
{
    public class VolunteerFormSubmissionRepository
        : GenericRepository<PromenadeContext, VolunteerFormSubmission>,
        IVolunteerFormSubmissionRepository
    {
        public VolunteerFormSubmissionRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<VolunteerFormSubmissionRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddAsync(VolunteerFormSubmission submission)
        {
            await DbSet.AddAsync(submission);
        }

        public async Task SaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException duex)
            {
                throw new OcudaException($"Error inserting volunteer form submission: {duex.Message}",
                    duex);
            }
        }
    }
}