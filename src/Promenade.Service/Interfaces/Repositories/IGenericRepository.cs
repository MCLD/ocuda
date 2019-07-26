using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ocuda.Promenade.Service.Interfaces
{
    public interface IGenericRepository<TEntity, TKeyType>
        where TEntity : class
        where TKeyType : struct
    {
        Task<TEntity> FindAsync(TKeyType id);
    }
}