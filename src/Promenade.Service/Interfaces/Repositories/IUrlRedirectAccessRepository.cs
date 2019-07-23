using System.Threading.Tasks;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IUrlRedirectAccessRepository
    {
        Task AddAccessLogAsync(int redirectId);
    }
}
