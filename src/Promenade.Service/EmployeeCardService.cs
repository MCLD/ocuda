using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class EmployeeCardService : BaseService<EmployeeCardService>
    {
        private readonly IEmployeeCardDepartmentRepository _employeeCardDepartmentRepository;
        private readonly IEmployeeCardRequestRepository _employeeCardRequestRepository;
        private readonly LanguageService _languageService;

        public EmployeeCardService(ILogger<EmployeeCardService> logger,
            IDateTimeProvider dateTimeProvider,
            IEmployeeCardDepartmentRepository employeeCardDepartmentRepository,
            IEmployeeCardRequestRepository employeeCardRequestRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(employeeCardDepartmentRepository);
            ArgumentNullException.ThrowIfNull(employeeCardRequestRepository);
            ArgumentNullException.ThrowIfNull(languageService);

            _employeeCardDepartmentRepository = employeeCardDepartmentRepository;
            _employeeCardRequestRepository = employeeCardRequestRepository;
            _languageService = languageService;
        }

        public async Task AddRequestAsync(EmployeeCardRequest cardRequest)
        {
            ArgumentNullException.ThrowIfNull(cardRequest);

            cardRequest.CardNumber = cardRequest.CardNumber?.ToUpperInvariant();
            cardRequest.City = cardRequest.City.Trim().ToUpperInvariant();
            cardRequest.Email = cardRequest.Email.Trim();
            cardRequest.EmployeeNumber = cardRequest.EmployeeNumber.Trim();
            cardRequest.FirstName = cardRequest.FirstName.Trim().ToUpperInvariant();
            cardRequest.LanguageId = await _languageService.GetLanguageIdAsync(
                CultureInfo.CurrentCulture.Name);
            cardRequest.LastName = cardRequest.LastName.Trim().ToUpperInvariant();
            cardRequest.Phone = cardRequest.Phone.Trim();
            cardRequest.StreetAddress = cardRequest.StreetAddress.Trim().ToUpperInvariant();
            cardRequest.SubmittedAt = _dateTimeProvider.Now;
            cardRequest.ZipCode = cardRequest.ZipCode.Trim();

            await _employeeCardRequestRepository.AddSaveAsync(cardRequest);
        }

        public async Task<ICollection<EmployeeCardDepartment>> GetDepartmentsAsync()
        {
            return await _employeeCardDepartmentRepository.GetSelectableAsync();
        }
    }
}
