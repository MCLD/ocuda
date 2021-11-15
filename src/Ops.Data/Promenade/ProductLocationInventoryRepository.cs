using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class ProductLocationInventoryRepository
        : GenericRepository<PromenadeContext, ProductLocationInventory>,
        IProductLocationInventoryRepository
    {
        public ProductLocationInventoryRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<ProductRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ProductLocationInventory> GetByProductAndLocationAsync(int productId,
            int locationId)
        {
            return await DbSet
                .Where(_ => _.ProductId == productId && _.LocationId == locationId)
                .Include(_ => _.Location)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<ProductLocationInventory>> GetForProductAsync(
            int productId)
        {
            return await DbSet
                .Where(_ => _.ProductId == productId)
                .OrderBy(_ => _.Location.Name)
                .Include(_ => _.Location)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
