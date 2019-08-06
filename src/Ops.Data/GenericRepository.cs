using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Data
{
    public abstract class GenericRepository<TEntity, TKeyType>
        where TEntity : class
        where TKeyType : struct
    {
        protected readonly OpsContext _context;
        protected readonly ILogger _logger;

        private DbSet<TEntity> _dbSet;

        protected GenericRepository(OpsContext context, ILogger logger)
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

        public virtual async Task AddAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(ICollection<TEntity> entities)
        {
            await DbSet.AddRangeAsync(entities);
        }

        public virtual void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void Remove(TKeyType id)
        {
            DbSet.Remove(DbSet.Find(id));
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public virtual void UpdateRange(ICollection<TEntity> entities)
        {
            DbSet.UpdateRange(entities);
        }

        public virtual async Task<ICollection<TEntity>> 
            ToListAsync(params Expression<Func<TEntity, IComparable>>[] orderBys)
        {
            Contract.Requires(orderBys != null && orderBys.Any());

            return await DbSetOrdered(orderBys)
                .AsNoTracking()
                .ToListAsync();
        }

        public virtual async Task<int> CountAsync()
        {
            return await DbSet.CountAsync();
        }

        public virtual async Task<ICollection<TEntity>> ToListAsync(int skip,
            int take,
            params Expression<Func<TEntity, IComparable>>[] orderBys)
        {
            Contract.Requires(orderBys != null && orderBys.Any());

            return await DbSetOrdered(orderBys)
                .AsNoTracking()
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public virtual async Task<TEntity> FindAsync(TKeyType id)
        {
            var entity = await DbSet.FindAsync(id);
            _context.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public virtual async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        private IOrderedQueryable<TEntity> 
            DbSetOrdered(Expression<Func<TEntity, IComparable>>[] orderBys)
        {
            IOrderedQueryable<TEntity> query = null;

            foreach (var orderBy in orderBys)
            {
                query = query == null
                    ? DbSet.OrderBy(orderBy)
                    : query.ThenBy(orderBy);
            }

            return query;
        }
    }
}
