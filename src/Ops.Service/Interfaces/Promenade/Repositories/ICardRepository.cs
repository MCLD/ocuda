using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICardRepository : IGenericRepository<Card>
    {
        public Task<Card> AddWithOrderAsync(Card card);

        public Task<int> DeleteCard(int cardId);

        public Task<ICollection<Card>> GetByDeckAsync(int deckId);

        public Task<int> GetCountByDeckIdAsync(int deckId);

        public Task<int> GetDeckIdAsync(int cardId);

        public Task<ICollection<Card>> GetOrderInformationById(int cardId);
    }
}