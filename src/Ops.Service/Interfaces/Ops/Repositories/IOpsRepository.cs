using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IOpsRepository<TEntity, TKeyType> : IGenericRepository<TEntity>
        where TEntity : class
    {
        Task<TEntity> FindAsync(TKeyType id);
        void Remove(TKeyType id);
    }
}
