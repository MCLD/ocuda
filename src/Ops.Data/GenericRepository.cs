﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Utility.Data;

namespace Ocuda.Ops.Data
{
    public abstract class GenericRepository<TContext, TEntity>
        where TContext : DbContextBase
        where TEntity : class
    {
        protected readonly TContext _context;
        protected readonly ILogger _logger;

        private DbSet<TEntity> _dbSet;

        protected GenericRepository(Repository<TContext> repositoryFacade, ILogger logger)
        {
            if (repositoryFacade == null)
            {
                throw new ArgumentNullException(nameof(repositoryFacade));
            }

            _context = repositoryFacade.context;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected DbSet<TEntity> DbSet
        {
            get
            {
                return _dbSet ??= _context.Set<TEntity>();
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

        public virtual async Task<int> CountAsync()
        {
            return await DbSet.CountAsync();
        }

        public virtual void Remove(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void RemoveRange(ICollection<TEntity> entities)
        {
            DbSet.RemoveRange(entities);
        }

        public virtual async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public virtual async Task<ICollection<TEntity>>
            ToListAsync(params Expression<Func<TEntity, IComparable>>[] orderBys)
        {
            return orderBys == null || orderBys.Length == 0
                ? throw new ArgumentNullException(nameof(orderBys))
                : (ICollection<TEntity>)await DbSetOrdered(orderBys)
                .AsNoTracking()
                .ToListAsync();
        }

        public virtual async Task<ICollection<TEntity>> ToListAsync(int skip,
            int take,
            params Expression<Func<TEntity, IComparable>>[] orderBys)
        {
            return orderBys == null || orderBys.Length == 0
                ? throw new ArgumentNullException(nameof(orderBys))
                : (ICollection<TEntity>)await DbSetOrdered(orderBys)
                .AsNoTracking()
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Update(entity);
        }

        public virtual void UpdateRange(ICollection<TEntity> entities)
        {
            DbSet.UpdateRange(entities);
        }

        protected IOrderedQueryable<TEntity>
            DbSetOrdered(Expression<Func<TEntity, IComparable>>[] orderBys)
        {
            IOrderedQueryable<TEntity> query = null;

            if (orderBys != null)
            {
                foreach (var orderBy in orderBys)
                {
                    query = query == null
                        ? DbSet.OrderBy(orderBy)
                        : query.ThenBy(orderBy);
                }
            }

            return query;
        }
    }
}