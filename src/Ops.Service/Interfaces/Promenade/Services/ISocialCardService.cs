using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ISocialCardService
    {
        Task<ICollection<SocialCard>> GetListAsync();
        Task<DataWithCount<ICollection<SocialCard>>> GetPaginatedListAsync(BaseFilter filter);
        Task<SocialCard> GetByIdAsync(int id);
        Task<SocialCard> CreateAsync(SocialCard card);
        Task<SocialCard> EditAsync(SocialCard card);
        Task DeleteAsync(int id);
    }
}
