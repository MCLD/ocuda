using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class LocationInteriorImageRepository : GenericRepository<PromenadeContext, LocationInteriorImage>,
        ILocationInteriorImageRepository
    {
        public LocationInteriorImageRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationInteriorImageRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<LocationInteriorImage>> GetLocationInteriorImagesAsync(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId)
                .ToListAsync();
        }
    }
}