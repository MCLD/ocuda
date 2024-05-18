using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class LocationInteriorImageRepository : GenericRepository<PromenadeContext, LocationInteriorImage>,
            ILocationInteriorImageRepository
    {
        public LocationInteriorImageRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<LocationInteriorImageRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task FixInteriorImageSortOrder(int locationId)
        {
            var interiorImages = DbSet.Where(_ => _.LocationId == locationId).ToList();

            int counter = 0;
            foreach (var interiorImage in interiorImages
                .OrderBy(_ => _.SortOrder)
                .ThenBy(_ => _.ImagePath))
            {
                interiorImage.SortOrder = counter++;
            }
            UpdateRange(interiorImages);
            await SaveAsync();
        }

        public async Task<LocationInteriorImage> GetInteriorImageByIdAsync(int imageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == imageId)
                .SingleOrDefaultAsync();
        }

        public async Task<List<LocationInteriorImage>> GetLocationInteriorImagesAsync(int locationId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId)
                .OrderBy(_ => _.SortOrder)
                .ToListAsync();
        }

        public async Task<int> GetNextSortOrderAsync(int locationId)
        {
            var top = await DbSet
                .AsNoTracking()
                .Where(_ => _.LocationId == locationId)
                .MaxAsync(_ => _.SortOrder as int?) ?? -1;
            return top + 1;
        }
    }
}