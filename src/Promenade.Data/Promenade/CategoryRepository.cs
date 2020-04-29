using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class CategoryRepository
        : GenericRepository<PromenadeContext, Category>, ICategoryRepository
    {
        public CategoryRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CategoryRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<Category>> GetAllCategories()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Id)
                .ToListAsync();
        }
    }
}
