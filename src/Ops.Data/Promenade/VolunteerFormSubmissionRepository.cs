using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class VolunteerFormSubmissionRepository : GenericRepository<PromenadeContext, VolunteerFormSubmission>, IVolunteerFormSubmissionRepository
    {
        public VolunteerFormSubmissionRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<VolunteerFormSubmissionRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<VolunteerFormSubmission>> GetAllAsync(int locationId, int formId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId && _.FormId == formId)
                .OrderByDescending(_ => _.CreatedAt)
                .ToListAsync();
        }

        public async Task<VolunteerFormSubmission> GetByIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == id)
                .FirstOrDefaultAsync();
        }
    }
}