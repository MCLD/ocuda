using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

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

        public async Task<Product> GetByIdAsync(int productId)
        {
            return await DbSet.AsNoTracking().SingleOrDefaultAsync(_ => _.Id == productId);
        }

        public async Task<ICollection<Product>> GetBySegmentIdAsync(int segmentId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SegmentId == segmentId)
                .ToListAsync();
        }

        public async Task<CollectionWithCount<Product>> GetPaginatedListAsync(BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new CollectionWithCount<Product>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
