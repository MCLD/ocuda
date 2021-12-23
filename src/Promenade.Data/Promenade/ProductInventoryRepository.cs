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
    public class ProductInventoryRepository
        : GenericRepository<PromenadeContext, ProductLocationInventory>, IProductInventoryRepository

    {
        public ProductInventoryRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ProductInventoryRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<ProductLocationInventory>> GetAsync(int productId)
        {
            return await DbSet.AsNoTracking().Where(_ => _.ProductId == productId).ToListAsync();
        }
    }
}
