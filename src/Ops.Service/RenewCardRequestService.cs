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
    public class RenewCardRequestService : BaseService<RenewCardRequestService>,
        IRenewCardRequestService
    {
        private readonly IRenewCardRequestRepository _renewCardRequestRepository;

        public RenewCardRequestService(ILogger<RenewCardRequestService> logger,
            IHttpContextAccessor httpContext,
            IRenewCardRequestRepository renewCardRequestRepository)
            : base(logger, httpContext)
        {
            ArgumentNullException.ThrowIfNull(renewCardRequestRepository);

            _renewCardRequestRepository = renewCardRequestRepository;
        }

        public async Task<RenewCardRequest> GetRequestAsync(int id)
        {
            return await _renewCardRequestRepository.GetByIdAsync(id);
        }

        public async Task<int> GetRequestCountAsync(bool? isProcessed)
        {
            return await _renewCardRequestRepository.GetCountAsync(isProcessed);
        }

        public async Task<CollectionWithCount<RenewCardRequest>> GetRequestsAsync(
            RequestFilter filter)
        {
            return await _renewCardRequestRepository.GetPaginatedAsync(filter);
        }
    }
}
