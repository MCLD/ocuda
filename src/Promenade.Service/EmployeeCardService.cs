using System;
using System.Collections.Generic;
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

        public EmployeeCardService(ILogger<EmployeeCardService> logger,
            IDateTimeProvider dateTimeProvider,
            IEmployeeCardDepartmentRepository employeeCardDepartmentRepository,
            IEmployeeCardRequestRepository employeeCardRequestRepository)
            : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(employeeCardDepartmentRepository);
            ArgumentNullException.ThrowIfNull(employeeCardRequestRepository);

            _employeeCardDepartmentRepository = employeeCardDepartmentRepository;
            _employeeCardRequestRepository = employeeCardRequestRepository;
        }

        public async Task AddRequestAsync(EmployeeCardRequest cardRequest)
        {
            ArgumentNullException.ThrowIfNull(cardRequest);

            cardRequest.City = cardRequest.City.Trim().ToUpperInvariant();
            cardRequest.FirstName = cardRequest.FirstName.Trim().ToUpperInvariant();
            cardRequest.LastName = cardRequest.LastName.Trim().ToUpperInvariant();
            cardRequest.StreetAddress = cardRequest.StreetAddress.Trim().ToUpperInvariant();
            cardRequest.SubmittedAt = _dateTimeProvider.Now;

            await _employeeCardRequestRepository.AddSaveAsync(cardRequest);
        }

        public async Task<ICollection<EmployeeCardDepartment>> GetDepartmentsAsync()
        {
            return await _employeeCardDepartmentRepository.GetSelectableAsync();
        }
    }
}
