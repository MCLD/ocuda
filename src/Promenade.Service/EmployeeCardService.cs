using System;
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
        private readonly IEmployeeCardRequestRepository _employeeCardRequestRepository;

        public EmployeeCardService(ILogger<EmployeeCardService> logger,
            IDateTimeProvider dateTimeProvider,
            IEmployeeCardRequestRepository employeeCardRequestRepository)
            : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(nameof(employeeCardRequestRepository));

            _employeeCardRequestRepository = employeeCardRequestRepository;
        }

        public async Task AddRequestAsync(EmployeeCardRequest cardRequest)
        {
            ArgumentNullException.ThrowIfNull(cardRequest);

            cardRequest.SubmittedAt = _dateTimeProvider.Now;
            await _employeeCardRequestRepository.AddSaveAsync(cardRequest);
        }
    }
}
