using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Service.Models.RenewCard;
using Ocuda.PolarisHelper;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class RenewCardService : BaseService<RenewCardService>, IRenewCardService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailService _emailService;
        private readonly ILanguageService _languageService;
        private readonly IPolarisHelper _polarisHelper;
        private readonly IRenewCardRequestRepository _renewCardRequestRepository;
        private readonly IRenewCardResponseRepository _renewCardResponseRepository;
        private readonly IRenewCardResultRepository _renewCardResultRepository;

        public RenewCardService(ILogger<RenewCardService> logger,
            IHttpContextAccessor httpContext,
            IDateTimeProvider dateTimeProvider,
            IEmailService emailService,
            ILanguageService languageService,
            IPolarisHelper polarisHelper,
            IRenewCardRequestRepository renewCardRequestRepository,
            IRenewCardResponseRepository renewCardResponseRepository,
            IRenewCardResultRepository renewCardResultRepository)
            : base(logger, httpContext)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(emailService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(polarisHelper);
            ArgumentNullException.ThrowIfNull(renewCardRequestRepository);
            ArgumentNullException.ThrowIfNull(renewCardResponseRepository);
            ArgumentNullException.ThrowIfNull(renewCardResultRepository);

            _dateTimeProvider = dateTimeProvider;
            _emailService = emailService;
            _languageService = languageService;
            _polarisHelper = polarisHelper;
            _renewCardRequestRepository = renewCardRequestRepository;
            _renewCardResponseRepository = renewCardResponseRepository;
            _renewCardResultRepository = renewCardResultRepository;
        }

        public async Task<RenewCardResponse> CreateResponseAsync(RenewCardResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            response.CreatedAt = _dateTimeProvider.Now;
            response.CreatedBy = GetCurrentUserId();
            response.Name = response.Name.Trim();

            var maxSortOrder = await _renewCardResponseRepository.GetMaxSortOrderAsync();
            if (maxSortOrder.HasValue)
            {
                response.SortOrder = maxSortOrder.Value + 1;
            }

            await _renewCardResponseRepository.AddAsync(response);
            await _renewCardResponseRepository.SaveAsync();

            return response;
        }

        public async Task DeleteResponseAsync(int id)
        {
            var response = await _renewCardResponseRepository.FindAsync(id);

            var subsequentResponses = await _renewCardResponseRepository
                .GetSubsequentAsync(response.SortOrder);
            if (subsequentResponses.Count > 0)
            {
                subsequentResponses.ForEach(_ => _.SortOrder--);
                _renewCardResponseRepository.UpdateRange(subsequentResponses);
            }

            response.IsDeleted = true;
            _renewCardResponseRepository.Update(response);
            await _renewCardResponseRepository.SaveAsync();
        }

        public async Task DiscardRequestAsync(int id)
        {
            var request = await _renewCardRequestRepository.GetByIdAsync(id);

            if (request == null)
            {
                throw new OcudaException("Request does not exist");
            }
            else if (request.ProcessedAt.HasValue)
            {
                throw new OcudaException("Request has already been processed");
            }

            var now = _dateTimeProvider.Now;

            var result = new RenewCardResult
            {
                RenewCardRequestId = request.Id,
                CreatedAt = now,
                CreatedBy = GetCurrentUserId(),
                IsDiscarded = true
            };
            await _renewCardResultRepository.AddAsync(result);

            request.IsDiscarded = true;
            request.ProcessedAt = now;
            _renewCardRequestRepository.Update(request);

            await _renewCardResultRepository.SaveAsync();
            await _renewCardRequestRepository.SaveAsync();
        }

        public async Task<IEnumerable<RenewCardResponse>> GetAvailableResponsesAsync()
        {
            return await _renewCardResponseRepository.GetAvailableAsync();
        }

        public async Task<RenewCardResponse> GetResponseAsync(int id)
        {
            return await _renewCardResponseRepository.FindAsync(id);
        }

        public async Task<IEnumerable<RenewCardResponse>> GetResponsesAsync()
        {
            return await _renewCardResponseRepository.GetAllAsync();
        }

        public async Task<RenewCardResponse> GetResponseTextAsync(int responseId, int languageId)
        {
            var response = await _renewCardResponseRepository.FindAsync(responseId);
            if (!response.EmailSetupId.HasValue)
            {
                _logger.LogError($"Invalid card renewal response '{responseId}': no email setup set.");
                throw new OcudaException("Invalid response");
            }

            var language = await _languageService.GetActiveByIdAsync(languageId);

            var emailSetupText = await _emailService.GetSetupTextByLanguageAsync(
                response.EmailSetupId.Value,
                language.Name);

            if (emailSetupText == null)
            {
                var defaultLanguage = await _languageService.GetDefaultLanguageNameAsync();

                emailSetupText = await _emailService.GetSetupTextByLanguageAsync(
                    response.EmailSetupId.Value,
                    defaultLanguage);
                if (emailSetupText == null)
                {
                    _logger.LogError($"Invalid card renewal response '{responseId}': email setup {response.EmailSetupId} is missing text.");
                    throw new OcudaException("Invalid response");
                }
            }

            response.Text = emailSetupText.BodyText;

            return response;
        }

        public async Task<RenewCardResult> GetResultForRequestAsync(int requestId)
        {
            return await _renewCardResultRepository.GetForRequestAsync(requestId);
        }

        public async Task<bool> IsRequestAccepted(int requestId)
        {
            var responseType = await _renewCardResultRepository
                .GetRequestResponseTypeAsync(requestId);

            return responseType == RenewCardResponse.ResponseType.Accept;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Show end user error message rather than exception")]
        public async Task<ProcessResult> ProcessRequestAsync(int requestId,
            int responseId,
            string responseText,
            string customerName)
        {
            var request = await _renewCardRequestRepository
                .GetByIdAsync(requestId);
            if (request.ProcessedAt.HasValue)
            {
                throw new OcudaException("Request has already been processed.");
            }

            var response = await _renewCardResponseRepository.FindAsync(responseId);

            var processResult = new ProcessResult
            {
                Type = response.Type
            };

            if (response.Type == RenewCardResponse.ResponseType.Accept)
            {
                var renewResult = _polarisHelper.RenewCustomerRegistration(
                        request.Barcode,
                        request.Email);

                if (!renewResult.Success)
                {
                    throw new OcudaException("Unable to update the record in Polaris.");
                }

                processResult.EmailNotUpdated = renewResult.EmailNotUpdated;
            }

            var language = await _languageService.GetActiveByIdAsync(request.LanguageId);
            var tags = new Dictionary<string, string>
            {
                { Keys.RenewCard.CustomerBarcode, request.Barcode },
                { Keys.RenewCard.CustomerName, customerName }
            };

            var emailDetails = await _emailService.GetDetailsAsync(response.EmailSetupId.Value,
                language.Name,
                tags,
                responseText);

            var now = _dateTimeProvider.Now;

            request.ProcessedAt = now;
            _renewCardRequestRepository.Update(request);

            var result = new RenewCardResult
            {
                RenewCardRequestId = request.Id,
                RenewCardResponseId = response.Id,
                CreatedAt = now,
                CreatedBy = GetCurrentUserId(),
                ResponseText = emailDetails.BodyText
            };

            await _renewCardResultRepository.AddAsync(result);
            await _renewCardResultRepository.SaveAsync();
            await _renewCardRequestRepository.SaveAsync();

            emailDetails.ToEmailAddress = request.Email;
            emailDetails.ToName = customerName;

            try
            {
                var sentEmail = await _emailService.SendAsync(emailDetails);
                if (sentEmail != null)
                {
                    processResult.EmailSent = true;
                }
                else
                {
                    _logger.LogWarning("Card renewal email (setup {EmailSetupId}) failed sending to {EmailTo}",
                        response.EmailSetupId.Value,
                        request.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                        "Error sending email setup {EmailSetupId} to {EmailTo}: {ErrorMessage}",
                        response.EmailSetupId.Value,
                        emailDetails.ToEmailAddress,
                        ex.Message);
            }

            return processResult;
        }

        public async Task UpdateResponseAsync(RenewCardResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            var currentResponse = await _renewCardResponseRepository.FindAsync(response.Id);

            currentResponse.EmailSetupId = response.EmailSetupId;
            currentResponse.Name = response.Name.Trim();

            _renewCardResponseRepository.Update(currentResponse);
            await _renewCardResponseRepository.SaveAsync();
        }

        public async Task UpdateResponseSortOrderAsync(int id, bool increase)
        {
            var response = await _renewCardResponseRepository.FindAsync(id);

            int newSortOrder;
            if (increase)
            {
                newSortOrder = response.SortOrder + 1;
            }
            else
            {
                if (response.SortOrder == 0)
                {
                    throw new OcudaException("Response is already in the first position.");
                }
                newSortOrder = response.SortOrder - 1;
            }

            var responseInPosition = await _renewCardResponseRepository.GetBySortOrderAsync(
                newSortOrder)
                ?? throw new OcudaException("Response is already in the last position.");

            responseInPosition.SortOrder = response.SortOrder;
            response.SortOrder = newSortOrder;

            _renewCardResponseRepository.Update(response);
            _renewCardResponseRepository.Update(responseInPosition);
            await _renewCardResponseRepository.SaveAsync();
        }
    }
}