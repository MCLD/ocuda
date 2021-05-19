using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
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

        public Task CancelAsync(ScheduleRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            return CancelInternalAsync(request);
        }

        public async Task<ICollection<ScheduleRequest>> GetPendingNotificationsAsync()
        {
            return await _scheduleRequestRepository.GetPendingNotificationsAsync();
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

        public async Task<ScheduleRequest> SetClaimedAsync(int scheduleRequestId)
        {
            var request = await _scheduleRequestRepository.GetRequestAsync(scheduleRequestId);

            request.IsClaimed = true;

            _scheduleRequestRepository.Update(request);
            await _scheduleRequestRepository.SaveAsync();

            return request;
        }

        public Task SetFollowupSentAsync(ScheduleRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            return SetFollowupSentInternalAsync(request);
        }

        public Task SetNotificationSentAsync(ScheduleRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            return SetNotificationSentInternalAsync(request);
        }

        public async Task SetUnderwayAsync(int scheduleRequestId)
        {
            var request = await GetRequestAsync(scheduleRequestId);
            request.IsUnderway = true;
            _scheduleRequestRepository.Update(request);
            await _scheduleRequestRepository.SaveAsync();
        }

        public async Task UnclaimAsync(int scheduleRequestId)
        {
            var request = await _scheduleRequestRepository.GetRequestAsync(scheduleRequestId);
            request.IsClaimed = false;
            _scheduleRequestRepository.Update(request);
            await _scheduleRequestRepository.SaveAsync();
        }

        private async Task CancelInternalAsync(ScheduleRequest request)
        {
            request.IsCancelled = true;
            _scheduleRequestRepository.Update(request);
            await _scheduleRequestRepository.SaveAsync();
        }

        private async Task SetFollowupSentInternalAsync(ScheduleRequest request)
        {
            request.FollowupSentAt = DateTime.Now;
            _scheduleRequestRepository.Update(request);
            await _scheduleRequestRepository.SaveAsync();
        }

        private async Task SetNotificationSentInternalAsync(ScheduleRequest request)
        {
            request.NotificationSentAt = DateTime.Now;
            _scheduleRequestRepository.Update(request);
            await _scheduleRequestRepository.SaveAsync();
        }
    }
}