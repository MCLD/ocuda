using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ICardRepository : IGenericRepository<Card>
    {
        public Task<int[]> GetCardIdsByDeckAsync(int deckId);
    }
}