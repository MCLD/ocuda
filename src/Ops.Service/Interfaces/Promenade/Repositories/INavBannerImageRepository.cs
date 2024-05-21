using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface INavBannerImageRepository : IGenericRepository<NavBannerImage>
    {
        Task<int> CountAsync(int languageId, string filename);

        Task<ICollection<NavBannerImage>> GetAllByNavBannerIdAsync(int id);

        Task<NavBannerImage> GetByNavBannerIdAsync(int id, int languageId);
    }
}