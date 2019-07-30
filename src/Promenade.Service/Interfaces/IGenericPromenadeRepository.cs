using System.Threading.Tasks;

namespace Ocuda.Promenade.Service.Interfaces
{
    public interface IGenericPromenadeRepository<TEntity, TKeyType>
        where TEntity : class
        where TKeyType : struct
    {
        Task<TEntity> FindAsync(TKeyType id);
    }
}
