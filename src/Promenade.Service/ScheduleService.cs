using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class ScheduleService : BaseService<ScheduleService>
    {
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;
        private readonly IScheduleRequestRepository _scheduleRequestRepository;
        private readonly IScheduleRequestSubjectRepository _scheduleRequestSubjectRepository;
        private readonly IScheduleRequestTelephoneRepository _scheduleRequestTelephoneRepository;

        public ScheduleService(ILogger<ScheduleService> logger,
            IDateTimeProvider dateTimeProvider,
            IConfiguration config,
            IDistributedCache cache,
            IScheduleRequestRepository scheduleRequestRepository,
            IScheduleRequestSubjectRepository scheduleRequestSubjectRepository,
            IScheduleRequestTelephoneRepository scheduleRequestTelephoneRepository)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _scheduleRequestRepository = scheduleRequestRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestRepository));
            _scheduleRequestSubjectRepository = scheduleRequestSubjectRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestRepository));
            _scheduleRequestTelephoneRepository = scheduleRequestTelephoneRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestTelephoneRepository));
        }

        public async Task<IEnumerable<ScheduleRequestSubject>> GetSubjectsAsync(bool forceReload)
        {
            IEnumerable<ScheduleRequestSubject> subjects = null;
            var pageCacheDuration = GetPageCacheDuration(_config);

            if (pageCacheDuration.HasValue && !forceReload)
            {
                subjects = await GetFromCacheAsync<IEnumerable<ScheduleRequestSubject>>(_cache,
                    Utility.Keys.Cache.PromScheduleSubjects);
            }

            if (subjects?.Any() != true)
            {
                subjects = await _scheduleRequestSubjectRepository.GetAllAsync();
                await SaveToCacheAsync(_cache,
                    Utility.Keys.Cache.PromScheduleSubjects,
                    subjects,
                    pageCacheDuration);
            }

            return subjects;
        }

        public async Task<ScheduleRequest> AddAsync(ScheduleRequest scheduleRequest,
            string phone)
        {
            if (scheduleRequest == null)
            {
                throw new ArgumentNullException(nameof(scheduleRequest));
            }

            if (string.IsNullOrEmpty(phone))
            {
                throw new ArgumentNullException(phone);
            }

            var fixedPhone = Regex.Replace(phone, "[^0-9.]", "").Trim();

            var requestTelephone = await _scheduleRequestTelephoneRepository.GetAsync(fixedPhone)
                ?? await _scheduleRequestTelephoneRepository.AddSaveAsync(fixedPhone);

            scheduleRequest.CreatedAt = _dateTimeProvider.Now;
            scheduleRequest.ScheduleRequestTelephoneId = requestTelephone.Id;
            var addedRequest = await _scheduleRequestRepository.AddSaveAsync(scheduleRequest);

            addedRequest.ScheduleRequestTelephone = requestTelephone;

            if (!string.IsNullOrEmpty(addedRequest.Email))
            {
                //send email
            }

            return addedRequest;
        }
    }
}
