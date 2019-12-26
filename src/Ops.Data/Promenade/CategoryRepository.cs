using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class CategoryRepository 
        : GenericRepository<PromenadeContext, Category, int>, ICategoryRepository
    {
        public CategoryRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CategoryRepository> logger) : base(repositoryFacade, logger)
        {
        }
        public async Task<ICollection<Category>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return new DataWithCount<ICollection<Category>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public Category GetByClass(string categoryClass)
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => _.Class == categoryClass)
                .FirstOrDefault();
        }
    }
}
