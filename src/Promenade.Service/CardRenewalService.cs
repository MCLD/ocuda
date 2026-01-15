using System;
using System.Globalization;
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
        private LanguageService _languageService;

        public CardRenewalService(ILogger<CardRenewalService> logger,
            IDateTimeProvider dateTimeProvider,
            ICardRenewalRequestRepository cardRenewalRequestRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(cardRenewalRequestRepository);
            ArgumentNullException.ThrowIfNull(languageService);

            _cardRenewalRequestRepository = cardRenewalRequestRepository;
            _languageService = languageService;
        }

        public async Task CreateRequestAsync(CardRenewalRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            request.LanguageId = await _languageService.GetLanguageIdAsync(
                CultureInfo.CurrentCulture.Name);
            request.SubmittedAt = _dateTimeProvider.Now;
            await _cardRenewalRequestRepository.AddSaveAsync(request);
        }

        public async Task<CardRenewalRequest> GetPendingRequestAsync(int customerId)
        {
            return await _cardRenewalRequestRepository.GetPendingRequestAsync(customerId);
        }
    }
}
