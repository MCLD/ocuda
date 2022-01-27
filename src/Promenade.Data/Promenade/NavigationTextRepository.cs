using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class NavigationTextRepository : INavigationTextRepository
    {
        private readonly PromenadeContext _context;
        private DbSet<NavigationText> _dbSet;

        public NavigationTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade)
        {
            if (repositoryFacade == null || repositoryFacade.context == null)
            {
                throw new ArgumentNullException(nameof(repositoryFacade));
            }

            _context = repositoryFacade.context;
        }

        protected DbSet<NavigationText> DbSet
        {
            get
            {
                return _dbSet ??= _context.Set<NavigationText>();
            }
        }

        public async Task<NavigationText> GetByIdsAsync(int navigationId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.NavigationId == navigationId && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}
