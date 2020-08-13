using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ISocialCardRepository : IGenericRepository<SocialCard>
    {
        Task<SocialCard> FindAsync(int id);
        Task<DataWithCount<ICollection<SocialCard>>> GetPaginatedListAsync(BaseFilter filter);
    }
}
