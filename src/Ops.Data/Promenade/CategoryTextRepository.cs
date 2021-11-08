using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class CategoryTextRepository
        : GenericRepository<PromenadeContext, CategoryText>, ICategoryTextRepository
    {
        public CategoryTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CategoryTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CategoryText> GetByCategoryAndLanguageAsync(int categoryId,
            int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<CategoryText>> GetAllForCategoryAsync(int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<ICollection<string>> GetUsedLanguagesForCategoryAsync(int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId)
                .OrderByDescending(_ => _.Language.IsDefault)
                .ThenBy(_ => _.Language.Name)
                .Select(_ => _.Language.Name)
                .ToListAsync();
        }
    }
}
