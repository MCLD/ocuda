using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class ProductRepository
        : GenericRepository<PromenadeContext, Product>, IProductRepository
    {
        public ProductRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<ProductRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Product> GetActiveBySlugAsync(string slug)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && _.Slug == slug)
                .SingleOrDefaultAsync();
        }
    }
}
