using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ICardDetailRepository : IGenericRepository<CardDetail>
    {
        Task<CardDetail> GetByIds(int cardId, IEnumerable<int> languageIds);
    }
}