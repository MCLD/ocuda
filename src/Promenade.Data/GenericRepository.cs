using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Data;

namespace Ocuda.Promenade.Data
{
    public class GenericRepository<TContext, TEntity, TKeyType>
        where TContext : DbContextBase
        where TEntity : class
        where TKeyType : struct
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

            _context = repositoryFacade.context ?? throw new ArgumentNullException();
            _dateTimeProvider = repositoryFacade.dateTimeProvider;
            _logger = logger ?? throw new ArgumentNullException();
        }

        protected DbSet<TEntity> DbSet
        {
            get
            {
                return _dbSet ?? (_dbSet = _context.Set<TEntity>());
            }
        }

        public async Task<TEntity> FindAsync(TKeyType id)
        {
            var entity = await DbSet.FindAsync(id);
            _context.Entry(entity).State = EntityState.Detached;
            return entity;
        }
    }
}
