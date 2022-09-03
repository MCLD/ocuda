using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class CardRepository
            : GenericRepository<PromenadeContext, Card>, ICardRepository

    {
        public CardRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<CardRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<int[]> GetCardIdsByDeckAsync(int deckId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.DeckId == deckId)
                .OrderBy(_ => _.Order)
                .Select(_ => _.Id)
                .ToArrayAsync();
        }
    }
}