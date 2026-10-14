using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Data.Ops
{
    public class SectionCategoryRepository(
        Repository<OpsContext> repositoryFacade,
        ILogger<SectionCategoryRepository> logger)
        : GenericRepository<OpsContext, SectionCategory>(repositoryFacade, logger),
        ISectionCategoryRepository
    {
        public async Task<IEnumerable<Category>> GetCategoriesAsync(int sectionId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .Select(_ => _.Category)
                .ToListAsync();
        }

        public async Task<IEnumerable<Section>> GetSectionsAsync(int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId)
                .Select(_ => _.Section)
                .ToListAsync();
        }

        public async Task RemoveAsync(int sectionId, int categoryId)
        {
            var sectionCategory = await DbSet
                .SingleOrDefaultAsync(_ => _.CategoryId == categoryId && _.SectionId == sectionId);

            if (sectionCategory == null)
            {
                throw new OcudaException("Category not found attached to section.");
            }
            else
            {
                Remove(sectionCategory);
                await SaveAsync();
            }
        }
    }
}
