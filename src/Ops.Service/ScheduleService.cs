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

namespace Ocuda.Ops.Service
{
    public class ScheduleService : BaseService<ScheduleService>, IScheduleService
    {
        private readonly IScheduleClaimRepository _scheduleClaimRepository;
        private readonly IScheduleLogCallDispositionRepository _scheduleLogCallDispositionRepository;
        private readonly IScheduleLogRepository _scheduleLogRepository;
        private readonly IScheduleNotificationService _scheduleNotificationService;
        private readonly IScheduleRequestService _scheduleRequestService;

        public ScheduleService(ILogger<ScheduleService> logger,
            IHttpContextAccessor httpContextAccessor,
            IScheduleClaimRepository scheduleClaimRepository,
            IScheduleLogCallDispositionRepository scheduleLogCallDispositionRepository,
            IScheduleLogRepository scheduleLogRepository,
            IScheduleNotificationService scheduleNotificationService,
            IScheduleRequestService scheduleRequestService)
            : base(logger, httpContextAccessor)
        {
            _scheduleClaimRepository = scheduleClaimRepository
                ?? throw new ArgumentNullException(nameof(scheduleClaimRepository));
            _scheduleLogCallDispositionRepository = scheduleLogCallDispositionRepository
                ?? throw new ArgumentNullException(nameof(scheduleLogCallDispositionRepository));
            _scheduleLogRepository = scheduleLogRepository
                ?? throw new ArgumentNullException(nameof(scheduleLogRepository));
            _scheduleNotificationService = scheduleNotificationService
                ?? throw new ArgumentNullException(nameof(scheduleNotificationService));
            _scheduleRequestService = scheduleRequestService
                ?? throw new ArgumentNullException(nameof(scheduleRequestService));
        }

        public async Task<ScheduleClaim> ClaimAsync(int scheduleRequestId)
        {
            var updatedClaim = await _scheduleClaimRepository.AddSaveAsync(new ScheduleClaim
            {
                CreatedAt = DateTime.Now,
                ScheduleRequestId = scheduleRequestId,
                UserId = GetCurrentUserId()
            });

            var request = await _scheduleRequestService.SetClaimedAsync(scheduleRequestId);

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

            await _scheduleRequestService.UnclaimAsync(scheduleRequestId);

            await AddLogAsync(new ScheduleLog
            {
                Notes = "Request unclaimed.",
                ScheduleRequestId = scheduleRequestId
            });
        }

        public async Task CancelAsync(int scheduleRequestId)
        {
            var request = await _scheduleRequestService.GetRequestAsync(scheduleRequestId);

            if (!string.IsNullOrEmpty(request.Email)
                && request.ScheduleRequestSubject.CancellationEmailSetupId != null)
            {
                var sentEmail = await _scheduleNotificationService.SendCancellationAsync(request);
                if (sentEmail != null)
                {
                    request.CancellationSentAt = DateTime.Now;
                }
            }
            else
            {
                await AddLogAsync(new ScheduleLog
                {
                    Notes = "Cancelled, no email sent.",
                    ScheduleRequestId = scheduleRequestId,
                    IsCancelled = true
                });
            }

            request.IsCancelled = true;

            await _scheduleRequestService.CancelAsync(request);
        }

        public async Task<IEnumerable<ScheduleClaim>> GetClaimsAsync(int[] scheduleRequestIds)
        {
            return await _scheduleClaimRepository.GetClaimsAsync(scheduleRequestIds);
        }

        public async Task<IEnumerable<ScheduleClaim>> GetCurrentUserClaimsAsync()
        {
            return await _scheduleClaimRepository.GetClaimsForUserAsync(GetCurrentUserId());
        }
        public Task AddLogAsync(ScheduleLog log)
        {
            return AddLogAsync(log, false);
        }

        public async Task AddLogAsync(ScheduleLog log, bool setUnderway)
        {
            if (log == null)
            {
                throw new ArgumentNullException(nameof(log));
            }

            ScheduleClaim claim = null;

            log.CreatedAt = DateTime.Now;
            log.UserId = GetCurrentUserId();
            if (log.IsComplete || log.IsCancelled)
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

            if (log.IsComplete)
            {
                var request = await _scheduleRequestService.GetRequestAsync(log.ScheduleRequestId);
                bool handled = false;
                if (request?.ScheduleRequestSubject.FollowupEmailSetupId != null)
                {
                    handled = await _scheduleNotificationService.SendFollowupAsync(request);
                }
                if (!handled)
                {
                    await AddLogAsync(new ScheduleLog
                    {
                        Notes = "Completed, no email sent.",
                        ScheduleRequestId = log.ScheduleRequestId,
                        IsCancelled = true
                    });
                }
            }
            else if (setUnderway)
            {
                await _scheduleRequestService.SetUnderwayAsync(log.ScheduleRequestId);
            }

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
