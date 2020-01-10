using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ISocialCardRepository : IGenericRepository<SocialCard>
    {
        Task<SocialCard> FindAsync(int id);
    }
}
