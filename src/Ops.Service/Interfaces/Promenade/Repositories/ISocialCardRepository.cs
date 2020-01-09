using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ISocialCardRepository : IGenericRepository<SocialCard>
    {
        Task<SocialCard> FindAsync(int id);
        Task<DataWithCount<ICollection<SocialCard>>> GetPaginatedListAsync(BaseFilter filter);
    }
}
