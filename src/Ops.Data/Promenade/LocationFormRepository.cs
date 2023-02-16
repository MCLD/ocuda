using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class LocationFormRepository : GenericRepository<PromenadeContext, LocationForm>, ILocationFormRepository
    {
        public LocationFormRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationFormRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveLocationForm(int locationId, int formId)
        {
            var locationForm = new LocationForm { LocationId = locationId, FormId = formId };
            await AddAsync(locationForm);
            await SaveAsync();
        }

        public async Task<LocationForm> FindAsync(int locationId, int formId)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.LocationId == locationId && _.FormId == formId);
        }

        public async Task RemoveSaveAsync(LocationForm locationForm)
        {
            Remove(locationForm);
            await SaveAsync();
        }
    }
}