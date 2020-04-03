using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service
{
    public class ScheduleRequestService
        : BaseService<ScheduleRequestService>, IScheduleRequestService
    {
        private readonly IScheduleRequestRepository _scheduleRequestRepository;

        public ScheduleRequestService(ILogger<ScheduleRequestService> logger,
            IHttpContextAccessor httpContextAccessor,
            IScheduleRequestRepository scheduleRequestRepository)
            : base(logger, httpContextAccessor)
        {
            _scheduleRequestRepository = scheduleRequestRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestRepository));
        }

        public async Task<ScheduleRequest> GetRequestAsync(int requestId)
        {
            return await _scheduleRequestRepository.GetRequestAsync(requestId);
        }

        public async Task<IEnumerable<ScheduleRequest>> GetRequestsAsync(DateTime requestedDate)
        {
            return await _scheduleRequestRepository.GetRequestsAsync(requestedDate);
        }

        public async Task<IEnumerable<ScheduleRequest>> GetUnclaimedRequestsAsync()
        {
            return await _scheduleRequestRepository.GetUnclaimedRequestsAsync();
        }
    }
}
