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
        private const int HoursInADay = 24;
        private const int SuggestedTimesTake = 3;

        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;
        private readonly IScheduleRequestRepository _scheduleRequestRepository;
        private readonly IScheduleRequestLimitRepository _scheduleRequestLimitRepository;
        private readonly IScheduleRequestSubjectRepository _scheduleRequestSubjectRepository;
        private readonly IScheduleRequestTelephoneRepository _scheduleRequestTelephoneRepository;

        public ScheduleService(ILogger<ScheduleService> logger,
            IDateTimeProvider dateTimeProvider,
            IConfiguration config,
            IDistributedCache cache,
            IScheduleRequestRepository scheduleRequestRepository,
            IScheduleRequestLimitRepository scheduleRequestLimitRepository,
            IScheduleRequestSubjectRepository scheduleRequestSubjectRepository,
            IScheduleRequestTelephoneRepository scheduleRequestTelephoneRepository)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _scheduleRequestRepository = scheduleRequestRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestRepository));
            _scheduleRequestLimitRepository = scheduleRequestLimitRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestLimitRepository));
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

        public Task<ScheduleRequest> AddAsync(ScheduleRequest scheduleRequest,
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

            return AddAsyncInternal(scheduleRequest, phone);
        }

        private async Task<ScheduleRequest> AddAsyncInternal(ScheduleRequest scheduleRequest,
            string phone)
        {
            var fixedPhone = Regex.Replace(phone, "[^0-9.]", "").Trim();

            var requestTelephone = await _scheduleRequestTelephoneRepository.GetAsync(fixedPhone)
                ?? await _scheduleRequestTelephoneRepository.AddSaveAsync(fixedPhone);

            scheduleRequest.CreatedAt = _dateTimeProvider.Now;
            scheduleRequest.ScheduleRequestTelephoneId = requestTelephone.Id;
            var addedRequest = await _scheduleRequestRepository.AddSaveAsync(scheduleRequest);

            addedRequest.ScheduleRequestTelephone = requestTelephone;

            return addedRequest;
        }

        public async Task<bool> IsRequestOverLimitAsync(DateTime requestTime)
        {
            var limit = await _scheduleRequestLimitRepository.GetTimeSlotLimitAsync(requestTime);
            if (limit.HasValue)
            {
                var requestCount = await _scheduleRequestRepository
                    .GetTimeSlotCountAsync(requestTime);
                if (requestCount >= limit.Value)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<ICollection<DateTime>> GetDaySuggestedTimesAsync(DateTime requestTime,
            double startHour,
            double availableHours,
            DateTime firstAvailable)
        {
            var dayLimits = await _scheduleRequestLimitRepository
                .GetLimitsForDayAsync(requestTime.DayOfWeek);

            var dayRequestCounts = await _scheduleRequestRepository
                .GetDayRequestCountsAsync(requestTime, firstAvailable);

            var availableTimes = new List<DateTime>();

            int hoursCount = 0;
            for (int hour = (int)startHour; hour < Math.Ceiling(startHour + availableHours); hour++)
            {
                hoursCount++;

                var dayHour = hour % HoursInADay;

                var time = requestTime.Date.AddHours(hour);

                if (time.Date > firstAvailable.Date || time.Hour >= firstAvailable.Hour)
                {
                    var requestCount = dayRequestCounts
                        .Where(_ => _.Data == dayHour)
                        .Select(_ => _.Count)
                        .SingleOrDefault();

                    var requestLimit = dayLimits
                        .Where(_ => _.Hour == dayHour)
                        .Select(_ => (int?)_.Limit)
                        .SingleOrDefault();

                    if (requestLimit == null || requestCount < requestLimit)
                    {
                        availableTimes.Add(time);
                    }
                }

                if (hoursCount >= HoursInADay)
                {
                    break;
                }
            }

            availableTimes = availableTimes
                .OrderBy(_ => Math.Abs(_.Hour - requestTime.Hour))
                .Take(SuggestedTimesTake)
                .OrderBy(_ => _.Hour)
                .ToList();

            return availableTimes;
        }

        public async Task<ICollection<DateTime>> GetHourSuggestedTimesAsync(DateTime requestTime,
            DateTime firstAvailable,
            List<DayOfWeek> blockedDays)
        {
            var hourLimits = await _scheduleRequestLimitRepository
                .GetLimitsForHourAsync(requestTime.Hour);

            var hourRequestCounts = await _scheduleRequestRepository
                .GetHourRequestCountsAsync(requestTime, firstAvailable);

            var availableTimes = new List<DateTime>();

            var firstDate = firstAvailable.Date;
            var lastDate = firstAvailable.Date.AddDays(Enum.GetNames(typeof(DayOfWeek)).Length);

            for (DateTime date = firstDate; date.Date <= lastDate.Date; date = date.AddDays(1))
            {
                if (!blockedDays.Contains(date.DayOfWeek))
                {
                    var requestCount = hourRequestCounts
                        .Where(_ => _.Data.Date == date.Date)
                        .Select(_ => _.Count)
                        .SingleOrDefault();

                    var requestLimit = hourLimits
                        .Where(_ => _.DayOfWeek == date.DayOfWeek)
                        .Select(_ => (int?)_.Limit)
                        .SingleOrDefault();

                    if (requestLimit == null || requestCount < requestLimit)
                    {
                        availableTimes.Add(date.Date + requestTime.TimeOfDay);
                    }
                }
            }

            availableTimes = availableTimes
                .OrderBy(_ => (_.Date - requestTime.Date).Duration())
                .Take(SuggestedTimesTake)
                .OrderBy(_ => _.Date)
                .ToList();

            return availableTimes;
        }
    }
}
