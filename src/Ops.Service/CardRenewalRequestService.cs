using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class CardRenewalRequestService : BaseService<CardRenewalRequestService>,
        ICardRenewalRequestService
    {
        private readonly ICardRenewalRequestRepository _cardRenewalRequestRepository;

        public CardRenewalRequestService(ILogger<CardRenewalRequestService> logger,
            IHttpContextAccessor httpContext,
            ICardRenewalRequestRepository cardRenewalRequestRepository)
            : base(logger, httpContext)
        {
            ArgumentNullException.ThrowIfNull(cardRenewalRequestRepository);

            _cardRenewalRequestRepository = cardRenewalRequestRepository;
        }

        public async Task<CardRenewalRequest> GetRequestAsync(int id)
        {
            return await _cardRenewalRequestRepository.GetByIdAsync(id);
        }

        public async Task<int> GetRequestCountAsync(bool? isProcessed)
        {
            return await _cardRenewalRequestRepository.GetCountAsync(isProcessed);
        }

        public async Task<CollectionWithCount<CardRenewalRequest>> GetRequestsAsync(
            RequestFilter filter)
        {
            return await _cardRenewalRequestRepository.GetPaginatedAsync(filter);
        }
    }
}
