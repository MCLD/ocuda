using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface IRepository<TEntity, TKeyType>
        where TEntity : class
        where TKeyType : struct
    {
        Task AddAsync(TEntity entity);
        Task<int> CountAsync();
        Task<TEntity> FindAsync(TKeyType id);
        void Remove(TEntity entity);
        void Remove(TKeyType id);
        Task SaveAsync();
        Task<ICollection<TEntity>> ToListAsync(int skip, int take, params Expression<Func<TEntity, IComparable>>[] orderBys);
        Task<ICollection<TEntity>> ToListAsync(params Expression<Func<TEntity, IComparable>>[] orderBys);
        void Update(TEntity entity);
    }
}