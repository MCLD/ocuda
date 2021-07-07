using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Data.Promenade
{
    public class NavigationTextRepository : INavigationTextRepository
    {
        protected readonly PromenadeContext _context;
        protected readonly ILogger _logger;
        protected readonly IDateTimeProvider _dateTimeProvider;

        private DbSet<NavigationText> _dbSet;

        public NavigationTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<NavigationTextRepository> logger)
        {
            if (repositoryFacade == null || repositoryFacade.context == null)
            {
                throw new ArgumentNullException(nameof(repositoryFacade));
            }

            _context = repositoryFacade.context;
            _dateTimeProvider = repositoryFacade.dateTimeProvider;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected DbSet<NavigationText> DbSet
        {
            get
            {
                return _dbSet ??= _context.Set<NavigationText>();
            }
        }

        public async Task<NavigationText> FindAsync(int id, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == id && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}
