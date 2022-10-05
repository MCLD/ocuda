using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class CardDetailRepository
            : GenericRepository<PromenadeContext, CardDetail>, ICardDetailRepository

    {
        public CardDetailRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<CardDetailRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CardDetail> GetByIds(int cardId, IEnumerable<int> languageIds)
        {
            if (languageIds == null)
            {
                throw new ArgumentNullException(nameof(languageIds));
            }

            foreach (var languageId in languageIds)
            {
                var cardDetail = await DbSet
                    .AsNoTracking()
                    .Where(_ => _.CardId == cardId && _.LanguageId == languageId)
                    .SingleOrDefaultAsync();

                if (cardDetail != null)
                {
                    return cardDetail;
                }
            }

            return null;
        }
    }
}