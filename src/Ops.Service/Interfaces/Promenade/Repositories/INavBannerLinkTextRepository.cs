using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface INavBannerLinkTextRepository : IGenericRepository<NavBannerLinkText>
    {
        Task<List<NavBannerLinkText>> GetAllLanguageTextsAsync(int navBannerLinkId);
        Task<NavBannerLinkText> GetLinkTextAsync(int navBannerLinkId, int languageId);
    }
}
