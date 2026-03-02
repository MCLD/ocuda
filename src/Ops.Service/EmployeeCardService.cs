using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.PolarisHelper;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class EmployeeCardService : BaseService<EmployeeCardService>, IEmployeeCardService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailService _emailService;
        private readonly IEmployeeCardNoteRepository _employeeCardNoteRepository;
        private readonly IEmployeeCardRequestRepository _employeeCardRequestRepository;
        private readonly IEmployeeCardResultRepository _employeeCardResultRepository;
        private readonly IEmployeeCardRequestService _employeeCardRequestService;
        private readonly ILanguageService _languageService;
        private readonly IPolarisHelper _polarisHelper;
        private readonly ISiteSettingService _siteSettingService;

        public EmployeeCardService(ILogger<EmployeeCardService> logger,
            IHttpContextAccessor httpContext,
            IDateTimeProvider dateTimeProvider,
            IEmailService emailService,
            IEmployeeCardNoteRepository employeeCardNoteRepository,
            IEmployeeCardRequestRepository employeeCardRequestRepository,
            IEmployeeCardResultRepository employeeCardResultRepository,
            IEmployeeCardRequestService employeeCardRequestService,
            ILanguageService languageService,
            IPolarisHelper polarisHelper,
            ISiteSettingService siteSettingService)
            : base(logger, httpContext)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(emailService);
            ArgumentNullException.ThrowIfNull(employeeCardNoteRepository);
            ArgumentNullException.ThrowIfNull(employeeCardRequestRepository);
            ArgumentNullException.ThrowIfNull(employeeCardResultRepository);
            ArgumentNullException.ThrowIfNull(employeeCardRequestService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(polarisHelper);
            ArgumentNullException.ThrowIfNull(siteSettingService);

            _dateTimeProvider = dateTimeProvider;
            _emailService = emailService;
            _employeeCardNoteRepository = employeeCardNoteRepository;
            _employeeCardRequestRepository = employeeCardRequestRepository;
            _employeeCardResultRepository = employeeCardResultRepository;
            _employeeCardRequestService = employeeCardRequestService;
            _languageService = languageService;
            _polarisHelper = polarisHelper;
            _siteSettingService = siteSettingService;
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
            Justification = "Show end user error message rather than exception")]
        public async Task<bool?> ProcessRequestAsync(int requestId,
            string cardNumber,
            EmployeeCardResult.ResultType type)
        {
            var request = await _employeeCardRequestRepository.GetByIdAsync(requestId);
            if (request == null)
            {
                throw new OcudaException("Request has already been processed or does not exist");
            }

            if (type == EmployeeCardResult.ResultType.CardCreated)
            {
                var expirationDate = _dateTimeProvider.Now.AddYears(1);
                var userField = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.EmployeeSignup.RegistrationUserField);

                var customer = new Customer
                {
                    Addresses =
                    [
                        new CustomerAddress
                            {
                                City = request.City,
                                CountryId = await _siteSettingService.GetSettingIntAsync(Ops.Models
                                    .Keys.SiteSetting.EmployeeSignup.RegistrationCountryId),
                                County = await _siteSettingService.GetSettingStringAsync(Ops.Models
                                    .Keys.SiteSetting.EmployeeSignup.RegistrationCounty),
                                PostalCode = request.ZipCode,
                                State = await _siteSettingService.GetSettingStringAsync(Ops.Models
                                    .Keys.SiteSetting.EmployeeSignup.RegistrationState),
                                StreetAddressOne = request.StreetAddress
                            }
                    ],
                    AddressVerificationDate = expirationDate,
                    CustomerCodeId = await _siteSettingService.GetSettingIntAsync(
                        Ops.Models.Keys.SiteSetting.EmployeeSignup.RegistrationCustomerCode),
                    CustomerIdNumber = cardNumber,
                    BirthDate = request.BirthDate,
                    EmailAddress = request.Email,
                    ExpirationDate = expirationDate,
                    NameFirst = request.FirstName,
                    NameLast = request.LastName,
                    PhoneNumber = request.Phone,
                    UserDefinedField1 = $"{userField} #{request.EmployeeNumber}".Trim(),
                    UserDefinedField4 = userField,
                    UserDefinedField5 = await _employeeCardRequestService
                        .GetDepartmentNameAsync(request.DepartmentId)
                };

                var createResult = _polarisHelper.CreateCustomerRegistration(customer);

                if (!createResult.Success)
                {
                    var exceptionMessage = new StringBuilder("An error occurred while trying to register the Polaris account");
                    if (!string.IsNullOrWhiteSpace(createResult.ErrorMessage))
                    {
                        exceptionMessage.Append(CultureInfo.InvariantCulture, 
                            $": {createResult.ErrorMessage}");
                    }
                    throw new OcudaException(exceptionMessage.ToString());
                }
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
                LanguageId = request.LanguageId,
                LastName = request.LastName,
                EmployeeCardRequestId = request.Id,
                EmployeeNumber = request.EmployeeNumber,
                Phone = request.Phone,
                ProcessedAt = now,
                ProcessedBy = GetCurrentUserId(),
                Renewal = !string.IsNullOrWhiteSpace(request.CardNumber),
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
                _logger.LogError(ex, "Error removing employee card request {RequestId} while processing: {ErrorMessage}",
                    request.Id,
                    ex.Message);
                try
                {
                    _employeeCardResultRepository.Remove(cardResult);
                    await _employeeCardResultRepository.SaveAsync();
                }
                catch (Exception ex2)
                {
                    _logger.LogCritical(ex2, "Error deleting employee card result after remove to delete card request {RequestId}: {ErrorMessage}",
                        request.Id,
                        ex2.Message);
                    throw new OcudaException("A critical error occurred while trying to process this request, please contact an administrator to resolve this issue.");
                }

                throw new OcudaException("An error occurred while trying to process this request, please try again or contact an administrator if this error continues to occur.");
            }

            if (type != EmployeeCardResult.ResultType.ProcessedNoEmail)
            {
                var language = await _languageService.GetActiveByIdAsync(request.LanguageId);
                var tags = new Dictionary<string, string>
                {
                    { Keys.RenewCard.CustomerBarcode, cardNumber }
                };

                int emailSetupId;
                if (cardResult.Renewal)
                {
                    emailSetupId = await _siteSettingService.GetSettingIntAsync(
                        Ops.Models.Keys.SiteSetting.EmployeeSignup.RenewEmailSetupId);
                }
                else
                {
                    emailSetupId = await _siteSettingService.GetSettingIntAsync(
                        Ops.Models.Keys.SiteSetting.EmployeeSignup.NewEmailSetupId);
                }

                var emailDetails = await _emailService.GetDetailsAsync(
                    emailSetupId,
                    language.Name,
                    tags);

                emailDetails.ToEmailAddress = cardResult.Email;
                emailDetails.ToName = $"{cardResult.FirstName} {cardResult.LastName}";

                try
                {
                    var sentEmail = await _emailService.SendAsync(emailDetails);
                    if (sentEmail != null)
                    {
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Employee card email setup {EmailSetupId}) failed sending to {EmailTo}",
                            emailSetupId,
                            emailDetails.ToEmailAddress);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                            "Error sending email setup {EmailSetupId} to {EmailTo}: {ErrorMessage}",
                            emailSetupId,
                            emailDetails.ToEmailAddress,
                            ex.Message);
                }
                return false;
            }

            return null;
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