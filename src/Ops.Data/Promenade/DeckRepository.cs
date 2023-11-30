using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class DeckRepository : GenericRepository<PromenadeContext, Deck>,
        IDeckRepository
    {
        public DeckRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<DeckRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task DeleteDeckAsync(int deckId)
        {
            var deck = await DbSet.FindAsync(deckId);
            DbSet.Remove(deck);
            await _context.SaveChangesAsync();
        }

        public async Task FixOrderAsync(int deckId)
        {
            var cards = _context.Cards
                .Where(_ => _.DeckId == deckId)
                .OrderBy(_ => _.Order);

            int order = 1;
            foreach (var card in cards)
            {
                card.Order = order;
                order++;
            }

            _context.UpdateRange(cards);
            await _context.SaveChangesAsync();
        }

        public async Task<Deck> GetByIdAsync(int deckId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == deckId)
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetPageHeaderIdAsync(int deckId)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.DeckId == deckId)
                .Select(_ => (int?)_.PageLayout.PageHeaderId)
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetPageLayoutIdAsync(int deckId)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.DeckId == deckId)
                .Select(_ => (int?)_.PageLayoutId)
                .SingleOrDefaultAsync();
        }
    }
}