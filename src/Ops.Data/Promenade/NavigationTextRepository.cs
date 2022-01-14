using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class NavigationTextRepository
        : GenericRepository<PromenadeContext, NavigationText>, INavigationTextRepository
    {
        public NavigationTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<NavigationTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<NavigationText> GetByNavigationAndLanguageAsync(int navigationId,
            int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavigationId == navigationId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<NavigationText>> GetByNavigationIdsAsync(
            List<int> navigationIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => navigationIds.Contains(_.NavigationId))
                .ToListAsync();
        }

        public async Task<List<string>> GetUsedLanguageNamesByNavigationId(int navigationId)
        {
            return await _context.NavigationTexts
                .AsNoTracking()
                .Where(_ => _.NavigationId == navigationId)
                .OrderByDescending(_ => _.Language.IsDefault)
                .ThenBy(_ => _.Language.Name)
                .Select(_ => _.Language.Name)
                .ToListAsync();
        }
    }
}
