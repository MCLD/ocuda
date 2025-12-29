using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class EmployeeCardService : BaseService<EmployeeCardService>, IEmployeeCardService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmployeeCardRequestRepository _employeeCardRequestRepository;

        public EmployeeCardService(ILogger<EmployeeCardService> logger,
            IHttpContextAccessor httpContext,
            IDateTimeProvider dateTimeProvider,
            IEmployeeCardRequestRepository employeeCardRequestRepository)
            : base(logger, httpContext)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(employeeCardRequestRepository);

            _dateTimeProvider = dateTimeProvider;
            _employeeCardRequestRepository = employeeCardRequestRepository;
        }

        public async Task<EmployeeCardRequest> GetRequestAsync(int requestId)
        {
            return await _employeeCardRequestRepository.GetByIdAsync(requestId);
        }

        public async Task<int> GetRequestCountAsync(bool? isProcessed)
        {
            return await _employeeCardRequestRepository.GetCountAsync(isProcessed);
        }

        public async Task<CollectionWithCount<EmployeeCardRequest>> GetRequestsAsync(
            RequestFilter filter)
        {
            return await _employeeCardRequestRepository.GetPaginatedAsync(filter);
        }

        public async Task ProcessAsync(EmployeeCardRequest cardRequest)
        {
            ArgumentNullException.ThrowIfNull(cardRequest);

            var currentRequest = await _employeeCardRequestRepository.GetByIdAsync(cardRequest.Id);

            if (currentRequest.ProcessedAt.HasValue)
            {
                throw new OcudaException("Request has already been processed.");
            }

            currentRequest.CardNumber = cardRequest.CardNumber.Trim();
            currentRequest.Notes = cardRequest.Notes;
            currentRequest.ProcessedAt = _dateTimeProvider.Now;
            currentRequest.ProcessedBy = GetCurrentUserId();
        }

        public async Task UpdateNotesAsync(EmployeeCardRequest cardRequest)
        {
            ArgumentNullException.ThrowIfNull(cardRequest);

            var currentRequest = await _employeeCardRequestRepository.GetByIdAsync(cardRequest.Id);
            currentRequest.Notes = cardRequest.Notes.Trim();

            await _employeeCardRequestRepository.AddAsync(currentRequest);
            await _employeeCardRequestRepository.SaveAsync();
        }
    }
}
