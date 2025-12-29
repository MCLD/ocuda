using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class CardRenewalService : BaseService<CardRenewalService>
    {
        private ICardRenewalRequestRepository _cardRenewalRequestRepository;

        public CardRenewalService(ILogger<CardRenewalService> logger,
            IDateTimeProvider dateTimeProvider,
            ICardRenewalRequestRepository cardRenewalRequestRepository)
            : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(cardRenewalRequestRepository);

            _cardRenewalRequestRepository = cardRenewalRequestRepository;
        }

        public async Task CreateRequestAsync(CardRenewalRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            request.SubmittedAt = _dateTimeProvider.Now;
            await _cardRenewalRequestRepository.AddSaveAsync(request);
        }

        public async Task<CardRenewalRequest> GetPendingRequestAsync(int patronId)
        {
            return await _cardRenewalRequestRepository.GetPendingRequestAsync(patronId);
        }
    }
}
