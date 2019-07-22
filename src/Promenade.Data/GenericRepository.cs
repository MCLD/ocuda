using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ocuda.Promenade.Data
{
    public class GenericRepository<TEntity, TKeyType>
        where TEntity : class
        where TKeyType : struct
    {
        protected readonly PromenadeContext _context;
        protected readonly ILogger _logger;

        private DbSet<TEntity> _dbSet;

        internal GenericRepository(PromenadeContext context, ILogger logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected DbSet<TEntity> DbSet
        {
            get
            {
                return _dbSet ?? (_dbSet = _context.Set<TEntity>());
            }
        }

        public virtual async Task<TEntity> FindAsync(TKeyType id)
        {
            var entity = await DbSet.FindAsync(id);
            _context.Entry(entity).State = EntityState.Detached;
            return entity;
        }
    }
}
