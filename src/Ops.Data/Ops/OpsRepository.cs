using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Utility.Data;

namespace Ocuda.Ops.Data.Ops
{
    public abstract class OpsRepository<TContext, TEntity, TKeyType>
        : GenericRepository<TContext, TEntity>
        where TContext : DbContextBase
        where TEntity : class
    {
        protected OpsRepository(Repository<TContext> repositoryFacade, ILogger logger)
            : base(repositoryFacade, logger)
        {
        }

        public virtual async Task<TEntity> FindAsync(TKeyType id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public virtual void Remove(TKeyType id)
        {
            DbSet.Remove(DbSet.Find(id));
        }
    }
}
