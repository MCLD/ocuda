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
            var sectionCats = await _context.SectionCategories
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .ToListAsync();
            return await DbSet
                .AsNoTracking()
                .Where(_ => sectionCats.Select(__ => __.CategoryId).Contains(_.Id))
                .ToListAsync();
        }

        public Category GetCategoryByStub(string stub, int sectionId)
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => _.Stub.ToLower() == stub.ToLower()
                && _.SectionId == sectionId)
                .FirstOrDefault();
        }

        public async Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(
            BaseFilter filter, int sectionId)
        {
            return new DataWithCount<ICollection<Category>>
            {
                Count = await DbSet
                    .AsNoTracking()
                    .Where(_ => _.SectionId == sectionId)
                    .CountAsync(),
                Data = await DbSet
                    .AsNoTracking()
                    .Where(_=>_.SectionId == sectionId)
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
