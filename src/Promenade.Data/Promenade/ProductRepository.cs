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
    public class ProductRepository
        : GenericRepository<PromenadeContext, Product>, IProductRepository
    {
        public ProductRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ProductRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<Product>> GetAllNamesSlugsAsync()
        {
            return await DbSet.AsNoTracking()
                .Where(_ => _.IsVisibleToPublic)
                .Select(_ => new Product
                {
                    Name = _.Name,
                    Slug = _.Slug
                })
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<Product> GetAsync(string slug)
        {
            return await DbSet.AsNoTracking()
                .Where(_ => _.IsVisibleToPublic)
                .SingleOrDefaultAsync(_ => _.Slug == slug);
        }
    }
}
