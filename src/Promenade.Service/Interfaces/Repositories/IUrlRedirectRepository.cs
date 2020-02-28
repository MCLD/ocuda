using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IUrlRedirectRepository
    {
        Task<UrlRedirect> GetRedirectIdPathAsync(string path);
    }
}
