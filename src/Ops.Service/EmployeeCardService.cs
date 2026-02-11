using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class EmployeeCardService : BaseService<EmployeeCardService>, IEmployeeCardService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmployeeCardNoteRepository _employeeCardNoteRepository;
        private readonly IEmployeeCardRequestRepository _employeeCardRequestRepository;
        private readonly IEmployeeCardResultRepository _employeeCardResultRepository;
        private readonly IEmployeeCardRequestService _employeeCardRequestService;

        public EmployeeCardService(ILogger<EmployeeCardService> logger,
            IHttpContextAccessor httpContext,
            IDateTimeProvider dateTimeProvider,
            IEmployeeCardNoteRepository employeeCardNoteRepository,
            IEmployeeCardRequestRepository employeeCardRequestRepository,
            IEmployeeCardResultRepository employeeCardResultRepository,
            IEmployeeCardRequestService employeeCardRequestService)
            : base(logger, httpContext)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(employeeCardNoteRepository);
            ArgumentNullException.ThrowIfNull(employeeCardRequestRepository);
            ArgumentNullException.ThrowIfNull(employeeCardResultRepository);
            ArgumentNullException.ThrowIfNull(employeeCardRequestService);

            _dateTimeProvider = dateTimeProvider;
            _employeeCardNoteRepository = employeeCardNoteRepository;
            _employeeCardRequestRepository = employeeCardRequestRepository;
            _employeeCardResultRepository = employeeCardResultRepository;
            _employeeCardRequestService = employeeCardRequestService;
        }

        public async Task<EmployeeCardNote> GetRequestNoteAsync(int requestId)
        {
            return await _employeeCardNoteRepository.FindAsync(requestId);
        }

        public async Task<int> GetResultCountAsync()
        {
            return await _employeeCardResultRepository.CountAsync();
        }

        public async Task<EmployeeCardResult> GetResultAsync(int id)
        {
            var result = await _employeeCardResultRepository.FindAsync(id);

            if (result != null)
            {
                result.DepartmentName = await _employeeCardRequestService
                    .GetDepartmentNameAsync(result.DepartmentId);
            }

            return result;
        }

        public async Task<CollectionWithCount<EmployeeCardResult>> GetResultsAsync(
            BaseFilter filter)
        {
            return await _employeeCardResultRepository.GetPaginatedAsync(filter);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Any email failure should not interrupt emailing other staff members.")]
        public async Task<EmployeeCardResult> ProcessRequestAsync(int requestId,
            string cardNumber,
            EmployeeCardResult.ResultType type)
        {
            var request = await _employeeCardRequestRepository.GetByIdAsync(requestId);
            if (request == null)
            {
                throw new OcudaException("Request has already been processed or does not exist");
            }

            var now = _dateTimeProvider.Now;

            var cardResult = new EmployeeCardResult
            {
                BirthDate = request.BirthDate,
                CardNumber = cardNumber,
                City = request.City,
                DepartmentId = request.DepartmentId,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmployeeCardRequestId = request.Id,
                EmployeeNumber = request.EmployeeNumber,
                ExistingAccount = request.ExistingAccount,
                Phone = request.Phone,
                ProcessedAt = now,
                ProcessedBy = GetCurrentUserId(),
                StreetAddress = request.StreetAddress,
                SubmittedAt = request.SubmittedAt,
                Type = type,
                ZipCode = request.ZipCode
            };

            await _employeeCardResultRepository.AddAsync(cardResult);
            await _employeeCardResultRepository.SaveAsync();

            try
            {
                _employeeCardRequestRepository.Remove(request);
                await _employeeCardRequestRepository.SaveAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee card request {RequestId} while processing: {ErrorMessage}",
                    request.Id,
                    ex.Message);
                try
                {
                    _employeeCardResultRepository.Remove(cardResult);
                    await _employeeCardResultRepository.SaveAsync();
                }
                catch (Exception ex2)
                {
                    _logger.LogCritical(ex2, "Error removing employee card result after failing to delete card request {RequestId}: {ErrorMessage}",
                        request.Id,
                        ex2.Message);
                    throw new OcudaException("A critical error occurred while trying to process this request, please contact the web team to resolve this issue.");
                }

                throw new OcudaException("An error occurred while trying to process this request, please try again or contact the web team if this error continues to occur.");
            }

            return cardResult;
        }

        public async Task SetRequestNote(EmployeeCardNote note)
        {
            ArgumentNullException.ThrowIfNull(note);

            var currentNote = await _employeeCardNoteRepository
                .FindAsync(note.EmployeeCardRequestId);

            if (!string.IsNullOrWhiteSpace(note.StaffNote))
            {
                if (currentNote == null)
                {
                    note.StaffNote = note.StaffNote?.Trim();
                    await _employeeCardNoteRepository.AddAsync(note);
                }
                else
                {
                    currentNote.StaffNote = note.StaffNote?.Trim();
                    _employeeCardNoteRepository.Update(currentNote);
                }
            }
            else if (currentNote != null)
            {
                _employeeCardNoteRepository.Remove(currentNote);
            }

            await _employeeCardNoteRepository.SaveAsync();
        }
    }
}