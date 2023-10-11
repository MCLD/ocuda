using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class LocationFormRepository : GenericRepository<PromenadeContext, LocationForm>, ILocationFormRepository
    {
        public LocationFormRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationFormRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<LocationForm> FindAsync(int formId, int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.FormId == formId && _.LocationId == locationId);
        }
    }
}