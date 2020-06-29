using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;

namespace Ocuda.Ops.Service
{
    public class ScheduleService : BaseService<ScheduleService>, IScheduleService
    {
        private readonly IScheduleClaimRepository _scheduleClaimRepository;
        private readonly IScheduleLogCallDispositionRepository _scheduleLogCallDispositionRepository;
        private readonly IScheduleLogRepository _scheduleLogRepository;
        private readonly IScheduleRequestRepository _scheduleRequestRepository;

        public ScheduleService(ILogger<ScheduleService> logger,
            IHttpContextAccessor httpContextAccessor,
            IScheduleClaimRepository scheduleClaimRepository,
            IScheduleLogCallDispositionRepository scheduleLogCallDispositionRepository,
            IScheduleLogRepository scheduleLogRepository,
            IScheduleRequestRepository scheduleRequestRepository)
            : base(logger, httpContextAccessor)
        {
            _scheduleClaimRepository = scheduleClaimRepository
                ?? throw new ArgumentNullException(nameof(scheduleClaimRepository));
            _scheduleLogCallDispositionRepository = scheduleLogCallDispositionRepository
                ?? throw new ArgumentNullException(nameof(scheduleLogCallDispositionRepository));
            _scheduleLogRepository = scheduleLogRepository
                ?? throw new ArgumentNullException(nameof(scheduleLogRepository));
            _scheduleRequestRepository = scheduleRequestRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestRepository));
        }

        public async Task<ScheduleClaim> AddAsync(int scheduleRequestId)
        {
            var updatedClaim = await _scheduleClaimRepository.AddSaveAsync(new ScheduleClaim
            {
                CreatedAt = DateTime.Now,
                ScheduleRequestId = scheduleRequestId,
                UserId = GetCurrentUserId()
            });

            var request = await _scheduleRequestRepository.GetRequestAsync(scheduleRequestId);

            request.IsClaimed = true;
            _scheduleRequestRepository.Update(request);
            await _scheduleRequestRepository.SaveAsync();

            await AddLogAsync(new ScheduleLog
            {
                Notes = "Request claimed.",
                ScheduleRequestId = request.Id
            });

            return updatedClaim;
        }

        public async Task UnclaimAsync(int scheduleRequestId)
        {
            _scheduleClaimRepository.Remove(scheduleRequestId);
            await _scheduleClaimRepository.SaveAsync();

            var request = await _scheduleRequestRepository.GetRequestAsync(scheduleRequestId);
            request.IsClaimed = false;
            _scheduleRequestRepository.Update(request);
            await _scheduleRequestRepository.SaveAsync();

            await AddLogAsync(new ScheduleLog
            {
                Notes = "Request unclaimed.",
                ScheduleRequestId = request.Id
            });
        }

        public async Task<IEnumerable<ScheduleClaim>> GetClaimsAsync(int[] scheduleRequestIds)
        {
            return await _scheduleClaimRepository.GetClaimsAsync(scheduleRequestIds);
        }

        public async Task<IEnumerable<ScheduleClaim>> GetCurrentUserClaimsAsync()
        {
            return await _scheduleClaimRepository.GetClaimsForUserAsync(GetCurrentUserId());
        }

        public async Task AddLogAsync(ScheduleLog log)
        {
            ScheduleClaim claim = null;

            log.CreatedAt = DateTime.Now;
            log.UserId = GetCurrentUserId();
            if (log.IsComplete)
            {
                var claims = await _scheduleClaimRepository.GetClaimsAsync(new int[]
                {
                    log.ScheduleRequestId
                });
                claim = claims.SingleOrDefault();
                if (claim?.IsComplete == false)
                {
                    claim.IsComplete = true;
                }
                else
                {
                    claim = null;
                }
            }

            await _scheduleLogRepository.AddAsync(log);
            await _scheduleLogRepository.SaveAsync();

            if (claim != null)
            {
                _scheduleClaimRepository.Update(claim);
                await _scheduleClaimRepository.SaveAsync();
            }
        }

        public async Task<IEnumerable<ScheduleLog>> GetLogAsync(int scheduleRequestId)
        {
            return await _scheduleLogRepository.GetByScheduleRequestIdAsync(scheduleRequestId);
        }

        public async Task<IEnumerable<ScheduleLogCallDisposition>> GetCallDispositionsAsync()
        {
            return await _scheduleLogCallDispositionRepository.GetAllAsync();
        }
    }
}
