using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class LocationRepository
        : GenericRepository<PromenadeContext, Location>, ILocationRepository
    {
        public LocationRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationHoursRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Location> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity?.IsDeleted != false)
            {
                return null;
            }
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<List<Location>> GetAllLocations()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<int?> GetIdBySlugAsync(string slug)
        {
            return await DbSet
                .Where(_ => !_.IsDeleted && _.Stub == slug)
                .Select(_ => _.Id)
                .SingleOrDefaultAsync();
        }
    }
}