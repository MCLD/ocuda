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
    public class CardDetailRepository : GenericRepository<PromenadeContext, CardDetail>,
        ICardDetailRepository
    {
        public CardDetailRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<CardDetailRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<int> CardsUsingImageAsync(string filename)
        {
            return await DbSet.CountAsync(_ => _.Filename == filename);
        }

        public async Task DeleteCardDetails(int cardId)
        {
            var details = DbSet.Where(_ => _.CardId == cardId);
            DbSet.RemoveRange(details);
            await _context.SaveChangesAsync();
        }

        public async Task<ICollection<CardDetail>> GetByCardId(int cardId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CardId == cardId)
                .ToListAsync();
        }

        public async Task<ICollection<CardDetail>> GetByDeckLanguageAsync(int deckId, int languageId)
        {
            var details = await DbSet
                .AsNoTracking()
                .Where(_ => _.Card.DeckId == deckId && _.LanguageId == languageId)
                .OrderBy(_ => _.Card.Order)
                .ToListAsync();

            foreach (var detail in details)
            {
                detail.LanguageCount = await _context.CardDetails
                    .AsNoTracking()
                    .Where(_ => _.CardId == detail.CardId)
                    .CountAsync();
            }

            return details;
        }

        public async Task<CardDetail> GetByIds(int cardId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.Card)
                .SingleOrDefaultAsync(_ => _.CardId == cardId && _.LanguageId == languageId);
        }
    }
}