using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface INavBannerLinkTextRepository : IGenericRepository<NavBannerLinkText>
    {
        Task<NavBannerLinkText> FindAsync(int navBannerLinkId, int languageId);

        Task<ICollection<NavBannerLinkText>> GetAllLanguageTextsAsync(int id);
    }
}