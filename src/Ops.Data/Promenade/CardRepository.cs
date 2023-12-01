using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class CardRepository : GenericRepository<PromenadeContext, Card>,
        ICardRepository
    {
        public CardRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<CardRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Card> AddWithOrderAsync(Card card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            var largestOrder = await DbSet
                  .AsNoTracking()
                  .Where(_ => _.DeckId == card.DeckId)
                  .MaxAsync(_ => (int?)_.Order);

            card.Order = (largestOrder ?? 0) + 1;

            await DbSet.AddAsync(card);
            return card;
        }

        public async Task<int> DeleteCard(int cardId)
        {
            var card = await DbSet.FindAsync(cardId);
            int deckId = card.DeckId;
            DbSet.Remove(card);
            await _context.SaveChangesAsync();
            return deckId;
        }

        public async Task<ICollection<Card>> GetByDeckAsync(int deckId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.DeckId == deckId)
                .ToListAsync();
        }

        public async Task<int> GetCountByDeckIdAsync(int deckId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.DeckId == deckId)
                .CountAsync();
        }

        public async Task<int> GetDeckIdAsync(int cardId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == cardId)
                .Select(_ => _.DeckId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Card>> GetOrderInformationById(int cardId)
        {
            var requestedCard = await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Id == cardId);

            if (requestedCard == null)
            {
                return new List<Card>();
            }

            var neighborCards = await DbSet
                .AsNoTracking()
                .Where(_ => _.DeckId == requestedCard.DeckId
                    && (_.Order == requestedCard.Order - 1
                        || _.Order == requestedCard.Order + 1))
                .ToListAsync();

            neighborCards.Add(requestedCard);

            return neighborCards.OrderBy(_ => _.Order).ToList();
        }
    }
}