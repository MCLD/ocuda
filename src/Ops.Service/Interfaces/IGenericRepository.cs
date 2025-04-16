using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces
{
    public interface IGenericRepository<TEntity>
        where TEntity : class
    {
        Task AddAsync(TEntity entity);

        Task AddRangeAsync(ICollection<TEntity> entities);

        Task<int> CountAsync();

        void Remove(TEntity entity);

        void RemoveRange(ICollection<TEntity> entities);

        Task SaveAsync();

        Task<ICollection<TEntity>> ToListAsync(int skip, int take, params Expression<Func<TEntity, IComparable>>[] orderBys);

        Task<ICollection<TEntity>> ToListAsync(params Expression<Func<TEntity, IComparable>>[] orderBys);

        void Update(TEntity entity);

        void UpdateRange(ICollection<TEntity> entities);
    }
}