using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class CategoryTextRepository
        : GenericRepository<PromenadeContext, CategoryText>, ICategoryTextRepository
    {
        public CategoryTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<CategoryTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CategoryText> GetByIdsAsync(int categoryId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}
