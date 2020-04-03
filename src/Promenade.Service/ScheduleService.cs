using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class ScheduleService : BaseService<ScheduleService>
    {
        private readonly IScheduleRequestRepository _scheduleRequestRepository;
        private readonly IScheduleRequestSubjectRepository _scheduleRequestSubjectRepository;
        private readonly IScheduleRequestTelephoneRepository _scheduleRequestTelephoneRepository;

        public ScheduleService(ILogger<ScheduleService> logger,
            IDateTimeProvider dateTimeProvider,
            IScheduleRequestRepository scheduleRequestRepository,
            IScheduleRequestSubjectRepository scheduleRequestSubjectRepository,
            IScheduleRequestTelephoneRepository scheduleRequestTelephoneRepository)
            : base(logger, dateTimeProvider)
        {
            _scheduleRequestRepository = scheduleRequestRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestRepository));
            _scheduleRequestSubjectRepository = scheduleRequestSubjectRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestRepository));
            _scheduleRequestTelephoneRepository = scheduleRequestTelephoneRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestTelephoneRepository));
        }

        public async Task<IEnumerable<ScheduleRequestSubject>> GetSubjectsAsync()
        {
            return await _scheduleRequestSubjectRepository.GetAllAsync();
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
