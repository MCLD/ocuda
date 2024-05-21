using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface INavBannerRepository : IGenericRepository<NavBanner>
    {
        Task<NavBanner> GetByIdAsync(int id);
    }
}