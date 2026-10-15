using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class CategoryRepository(
        ServiceFacade.Repository<OpsContext> repositoryFacade,
        ILogger<CategoryRepository> logger)
        : OpsRepository<OpsContext, Category, int>(repositoryFacade, logger),
        ICategoryRepository
    {
        public async Task<List<Category>> GetCategoriesBySectionIdAsync(int sectionId)
        {
            return await _context.SectionCategories
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .Select(_ => _.Category)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryBySlugAsync(string slug)
        {
            return await DbSet.AsNoTracking().Where(_ => _.Slug == slug).SingleOrDefaultAsync();
        }

        public async Task<CollectionWithCount<Category>> PageAsync(BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new CollectionWithCount<Category>
            {
                Count = await query.CountAsync(),
                Data = await query.AsNoTracking()
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync(),
            };
        }

        public async Task<bool> SectionHasCategoryAsync(int categoryId, int sectionId)
        {
            return await _context.SectionCategories
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId && _.CategoryId == categoryId)
                .AnyAsync();
        }
    }
}
