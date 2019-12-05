using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class CategoryRepository : GenericRepository<OpsContext, Category, int>, ICategoryRepository
    {
        public CategoryRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<CategoryRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<Category>> GetCategoriesBySectionIdAsync(int sectionId)
        {
            return await _context.SectionCategories
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .Select(__ => __.Category)
                .ToListAsync();
        }

        public async Task<Category> GetCategoryByStubAsync(string stub)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub.ToLower() == stub.ToLower())
                .SingleOrDefaultAsync();
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
