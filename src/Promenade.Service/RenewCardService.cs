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
    public class RenewCardService : BaseService<RenewCardService>
    {
        private LanguageService _languageService;
        private IRenewCardRequestRepository _renewCardRequestRepository;

        public RenewCardService(ILogger<RenewCardService> logger,
            IDateTimeProvider dateTimeProvider,
            IRenewCardRequestRepository renewCardRequestRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(renewCardRequestRepository);
            ArgumentNullException.ThrowIfNull(languageService);

            _languageService = languageService;
            _renewCardRequestRepository = renewCardRequestRepository;
        }

        public async Task CreateRequestAsync(RenewCardRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            request.LanguageId = await _languageService.GetLanguageIdAsync(
                CultureInfo.CurrentCulture.Name);
            request.SubmittedAt = _dateTimeProvider.Now;
            await _renewCardRequestRepository.AddSaveAsync(request);
        }

        public async Task<RenewCardRequest> GetPendingRequestAsync(int customerId)
        {
            return await _renewCardRequestRepository.GetPendingRequestAsync(customerId);
        }
    }
}