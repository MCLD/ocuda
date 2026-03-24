using System.Threading.Tasks;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IEmediaAccessRepository
    {
        Task AddAccessLogAsync(int emediaId);
    }
}