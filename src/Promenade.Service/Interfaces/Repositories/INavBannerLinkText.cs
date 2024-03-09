using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface INavBannerLinkTextRepository : IGenericRepository<NavBannerLinkText>
    {
        Task<NavBannerLinkText> GetByLinkIdAsync(int navBannerLinkId, int languageId);
    }
}
