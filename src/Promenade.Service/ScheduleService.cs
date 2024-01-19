using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class ScheduleService : BaseService<ScheduleService>
    {
        private const int HoursInADay = 24;
        private const int SuggestedTimesTake = 3;
        private const double TimeBlockInterval = 0.5;

        private readonly IOcudaCache _cache;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LanguageService _languageService;
        private readonly IScheduleRequestLimitRepository _scheduleRequestLimitRepository;
        private readonly IScheduleRequestRepository _scheduleRequestRepository;
        private readonly IScheduleRequestSubjectRepository _scheduleRequestSubjectRepository;
        private readonly IScheduleRequestSubjectTextRepository _scheduleRequestSubjectTextRepository;
        private readonly IScheduleRequestTelephoneRepository _scheduleRequestTelephoneRepository;

        public ScheduleService(ILogger<ScheduleService> logger,
            IDateTimeProvider dateTimeProvider,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            IOcudaCache cache,
            IScheduleRequestLimitRepository scheduleRequestLimitRepository,
            IScheduleRequestRepository scheduleRequestRepository,
            IScheduleRequestSubjectRepository scheduleRequestSubjectRepository,
            IScheduleRequestSubjectTextRepository scheduleRequestSubjectTextRepository,
            IScheduleRequestTelephoneRepository scheduleRequestTelephoneRepository,
            LanguageService languageService) : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _scheduleRequestLimitRepository = scheduleRequestLimitRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestLimitRepository));
            _scheduleRequestRepository = scheduleRequestRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestRepository));
            _scheduleRequestSubjectRepository = scheduleRequestSubjectRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestRepository));
            _scheduleRequestSubjectTextRepository = scheduleRequestSubjectTextRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestSubjectTextRepository));
            _scheduleRequestTelephoneRepository = scheduleRequestTelephoneRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestTelephoneRepository));
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
            for (var hour = (int)startHour; hour < Math.Ceiling(startHour + availableHours); hour++)
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

            return availableTimes
                .OrderBy(_ => Math.Abs(_.Hour - requestTime.Hour))
                .Take(SuggestedTimesTake)
                .OrderBy(_ => _.Hour)
                .ToList();
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

            return availableTimes
                .OrderBy(_ => (_.Date - requestTime.Date).Duration())
                .Take(SuggestedTimesTake)
                .OrderBy(_ => _.Date)
                .ToList();
        }

        public async Task<IEnumerable<ScheduleRequestSubject>> GetSubjectsAsync(bool forceReload)
        {
            var doForceReload = forceReload;
            IEnumerable<ScheduleRequestSubject> subjects = null;
            var pageCacheDuration = GetPageCacheDuration(_config);

            if (pageCacheDuration == 0)
            {
                doForceReload = true;
            }

            if (!doForceReload)
            {
                subjects = await _cache
                    .GetObjectFromCacheAsync<IEnumerable<ScheduleRequestSubject>>(
                        Utility.Keys.Cache.PromScheduleSubjects);
            }

            if (subjects?.Any() != true)
            {
                subjects = (await _scheduleRequestSubjectRepository.GetAllAsync())
                    .Where(_ => _.IsActive)
                    .OrderBy(_ => _.OrderBy);
                await _cache.SaveToCacheAsync(Utility.Keys.Cache.PromScheduleSubjects,
                    subjects,
                    pageCacheDuration);
            }

            var currentCultureName = _httpContextAccessor
                .HttpContext
                .Features
                .Get<IRequestCultureFeature>()
                .RequestCulture
                .UICulture?
                .Name;

            int defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
            int? currentLanguageId = null;

            if (!string.IsNullOrWhiteSpace(currentCultureName))
            {
                currentLanguageId = await _languageService.GetLanguageIdAsync(currentCultureName);
            }

            foreach (var subject in subjects)
            {
                if (currentLanguageId.HasValue)
                {
                    subject.SubjectText = await GetSubjectTextAsync(doForceReload,
                        pageCacheDuration,
                        currentLanguageId.Value,
                        subject.Id);
                }

                if (string.IsNullOrEmpty(subject.SubjectText))
                {
                    subject.SubjectText = await GetSubjectTextAsync(doForceReload,
                        pageCacheDuration,
                        defaultLanguageId,
                        subject.Id);
                }
            }
            return subjects;
        }

        public async Task<IEnumerable<SelectListItem>> GetAvailableTimeBlocks(double startHour, double availableHours, DateTime requestedDate)
        {
            // Round up start hour to nearest time interval
            startHour = Math.Ceiling(startHour / TimeBlockInterval)
                * TimeBlockInterval;

            // Round down end hour to nearest time interval
            var endHour = Math.Floor((startHour + availableHours) / TimeBlockInterval)
                * TimeBlockInterval;

            var timeBlocks = new List<DateTime>();

            var dayRequests = await _scheduleRequestRepository.GetRequestsForDay(requestedDate);

            var dayLimits = (await _scheduleRequestLimitRepository
                .GetLimitsForDayAsync(requestedDate.DayOfWeek))
                .ToDictionary(_ => _.Hour, _ => _.Limit);

            for (double hour = startHour; hour <= endHour; hour += TimeBlockInterval)
            {
                var dayHour = hour % HoursInADay;

                var timeBlock = requestedDate.Date.AddHours(dayHour);

                var isHourAtLimit = dayLimits[timeBlock.Hour] == 0 || dayRequests.ContainsKey(timeBlock) && dayRequests[timeBlock] >= dayLimits[timeBlock.Hour];

                if (timeBlock > requestedDate && !isHourAtLimit)
                {
                    timeBlocks.Add(DateTime.Now.Date.AddHours(dayHour));
                }


                if (timeBlocks.Count >= HoursInADay / TimeBlockInterval)
                {
                    break;
                }
            }

            return timeBlocks
                .OrderBy(_ => _.TimeOfDay)
                .Select(_ => new SelectListItem
                {
                    Text = _.ToShortTimeString(),
                    Value = _.ToString(CultureInfo.CurrentCulture)
                });
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

        private async Task<string> GetSubjectTextAsync(bool forceReload,
            int pageCacheDuration,
            int languageId,
            int subjectId)
        {
            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromScheduleSubjectTexts,
                languageId,
                subjectId);

            string subjectText = null;

            if (!forceReload)
            {
                subjectText = await _cache.GetStringFromCache(cacheKey);
            }

            if (string.IsNullOrEmpty(subjectText))
            {
                subjectText = await _scheduleRequestSubjectTextRepository
                        .GetByIdsAsync(subjectId, languageId);

                if (!string.IsNullOrEmpty(subjectText) && pageCacheDuration > 0)
                {
                    await _cache.SaveToCacheAsync(cacheKey, subjectText, pageCacheDuration);
                }
            }

            return subjectText;
        }
    }
}