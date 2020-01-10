using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Data;

namespace Ocuda.Promenade.Data
{
    public class GenericRepository<TContext, TEntity>
        where TContext : DbContextBase
        where TEntity : class
    {
        protected readonly TContext _context;
        protected readonly ILogger _logger;
        protected readonly IDateTimeProvider _dateTimeProvider;

        private DbSet<TEntity> _dbSet;

        internal GenericRepository(Repository<TContext> repositoryFacade, ILogger logger)
        {
            if (repositoryFacade == null)
            {
                throw new ArgumentNullException(nameof(repositoryFacade));
            }

            _context = repositoryFacade.context
                ?? throw new ArgumentNullException(nameof(repositoryFacade));
            _dateTimeProvider = repositoryFacade.dateTimeProvider;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected DbSet<TEntity> DbSet
        {
            get
            {
                return _dbSet ?? (_dbSet = _context.Set<TEntity>());
            }
        }
    }
}
