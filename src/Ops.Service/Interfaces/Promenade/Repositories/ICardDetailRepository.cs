using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICardDetailRepository : IGenericRepository<CardDetail>
    {
        Task<int> CardsUsingImageAsync(string filename);

        Task DeleteCardDetails(int cardId);

        Task<ICollection<CardDetail>> GetByCardId(int cardId);

        Task<ICollection<CardDetail>> GetByDeckLanguageAsync(int dockId, int languageId);

        Task<CardDetail> GetByIds(int cardId, int languageId);
    }
}