using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;

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

        public Category GetCategoryByStub(string stub)
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => _.Stub.ToLower() == stub.ToLower())
                .FirstOrDefault();
        }

        public async Task<bool> SectionHasCategoryAsync(int categoryId, int sectionId)
        {
            return await _context.SectionCategories
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId
                    && _.CategoryId == categoryId)
                .AnyAsync();
        }

        public async Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(
            BaseFilter filter, int sectionId)
        {
            var sectionCats = _context.SectionCategories
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .ToList();
            return new DataWithCount<ICollection<Category>>
            {
                Count = await DbSet
                    .AsNoTracking()
                    .Where(_ => sectionCats.Any(__=>__.CategoryId == _.Id))
                    .CountAsync(),
                Data = await DbSet
                    .AsNoTracking()
                    .Where(_=> sectionCats.Any(__ => __.CategoryId == _.Id))
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

    }
}
