using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ISocialCardService
    {
        Task<DataWithCount<ICollection<SocialCard>>> GetPaginatedListAsync(BaseFilter filter);
        Task<SocialCard> GetByIdAsyn(int id);
        Task<SocialCard> CreateAsync(SocialCard card);
        Task<SocialCard> EditAsync(SocialCard card);
        Task DeleteAsync(int id);
    }
}
