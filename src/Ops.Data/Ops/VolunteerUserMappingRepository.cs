using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class VolunteerUserMappingRepository : GenericRepository<OpsContext, VolunteerFormUserMapping>, IVolunteerUserMappingRepository
    {
        public VolunteerUserMappingRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<VolunteerUserMappingRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveFormUserMappingAsync(int formId, int locationId, int userId)
        {
            await AddAsync(new VolunteerFormUserMapping
            {
                VolunteerFormId = formId,
                LocationId = locationId,
                UserId = userId
            });
            await SaveAsync();
        }

        public async Task<List<VolunteerFormUserMapping>> FindAsync(int locationId, int formId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId && _.VolunteerFormId == formId)
                .Include(_ => _.User)
                .ToListAsync();
        }

        public async Task<VolunteerFormUserMapping> FindAsync(int formId, int locationId, int userId)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.User)
                .FirstOrDefaultAsync(_ => _.LocationId == locationId && _.VolunteerFormId == formId && _.UserId == userId);
        }

        public async Task RemoveFormUserMappingAsync(int formId, int locationId, int userId)
        {
            var mappingToRemove = await FindAsync(formId, locationId, userId);
            Remove(mappingToRemove);
            await SaveAsync();
        }
    }
}