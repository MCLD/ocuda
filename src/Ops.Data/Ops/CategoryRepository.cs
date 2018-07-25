using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class CategoryRepository : GenericRepository<Models.Category, int>, ICategoryRepository
    {
        public CategoryRepository(OpsContext context, ILogger<CategoryRepository> logger)
            : base(context, logger)
        {

        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await DbSet
                    .AsNoTracking()
                    .Where(_ => string.Equals(_.Name, name, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefaultAsync();
        }

        public async Task<Category> GetDefaultAsync(BlogFilter filter)
        {
            return await DbSet
                    .AsNoTracking()
                    .Where(_ => _.CategoryType == filter.CategoryType
                             && _.SectionId == filter.SectionId
                             && _.IsDefault == true)
                    .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Category>> GetBySectionIdAsync(BlogFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.CategoryType.HasValue)
            {
                query = query.Where(_ => _.CategoryType == filter.CategoryType);
            }

            if (filter.SectionId.HasValue)
            {
                query = query.Where(_ => _.SectionId == filter.SectionId);
            }

            return await query
                    .OrderByDescending(_ => _.IsDefault)
                    .ThenBy(_ => _.Name)
                    .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(BlogFilter filter)
        {
            var query = DbSet.AsNoTracking().Where(_ => _.IsDefault == false);

            if (filter.CategoryType.HasValue)
            {
                query = query.Where(_ => _.CategoryType == filter.CategoryType);
            }

            if (filter.SectionId.HasValue)
            {
                query = query.Where(_ => _.SectionId == filter.SectionId);
            }

            return new DataWithCount<ICollection<Category>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<bool> IsDuplicateAsync(Category category)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Name == category.Name
                         && _.SectionId == category.SectionId
                         && _.CategoryType == category.CategoryType
                         && _.Id != category.Id)
                .AnyAsync();
        }
    }
}
