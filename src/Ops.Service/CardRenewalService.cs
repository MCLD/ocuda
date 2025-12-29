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
using Ocuda.Ops.Service.Models.CardRenewal;
using Ocuda.PolarisHelper;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class CardRenewalService : BaseService<CardRenewalService>,
        ICardRenewalService
    {
        private readonly ICardRenewalRequestRepository _cardRenewalRequestRepository;
        private readonly ICardRenewalResponseRepository _cardRenewalResponseRepository;
        private readonly ICardRenewalResultRepository _cardRenewalResultRepository;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IEmailService _emailService;
        private readonly ILanguageService _languageService;
        private readonly IPolarisHelper _polarisHelper;

        public CardRenewalService(ILogger<CardRenewalService> logger,
            IHttpContextAccessor httpContext,
            ICardRenewalRequestRepository cardRenewalRequestRepository,
            ICardRenewalResponseRepository cardRenewalResponseRepository,
            ICardRenewalResultRepository cardRenewalResultRepository,
            IDateTimeProvider dateTimeProvider,
            IEmailService emailService,
            ILanguageService languageService,
            IPolarisHelper polarisHelper)
            : base(logger, httpContext)
        {
            ArgumentNullException.ThrowIfNull(cardRenewalRequestRepository);
            ArgumentNullException.ThrowIfNull(cardRenewalResponseRepository);
            ArgumentNullException.ThrowIfNull(cardRenewalResultRepository);
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(emailService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(polarisHelper);

            _cardRenewalRequestRepository = cardRenewalRequestRepository;
            _cardRenewalResponseRepository = cardRenewalResponseRepository;
            _cardRenewalResultRepository = cardRenewalResultRepository;
            _dateTimeProvider = dateTimeProvider;
            _emailService = emailService;
            _languageService = languageService;
            _polarisHelper = polarisHelper;
        }

        public async Task<CardRenewalResponse> CreateResponseAsync(CardRenewalResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            response.CreatedAt = _dateTimeProvider.Now;
            response.CreatedBy = GetCurrentUserId();
            response.Name = response.Name.Trim();

            var maxSortOrder = await _cardRenewalResponseRepository.GetMaxSortOrderAsync();
            if (maxSortOrder.HasValue)
            {
                response.SortOrder = maxSortOrder.Value + 1;
            }

            await _cardRenewalResponseRepository.AddAsync(response);
            await _cardRenewalResponseRepository.SaveAsync();

            return response;
        }

        public async Task DeleteResponseAsync(int id)
        {
            var response = await _cardRenewalResponseRepository.FindAsync(id);

            var subsequentResponses = await _cardRenewalResponseRepository
                .GetSubsequentAsync(response.SortOrder);
            if (subsequentResponses.Count > 0)
            {
                subsequentResponses.ForEach(_ => _.SortOrder--);
                _cardRenewalResponseRepository.UpdateRange(subsequentResponses);
            }

            response.IsDeleted = true;
            _cardRenewalResponseRepository.Update(response);
            await _cardRenewalResponseRepository.SaveAsync();
        }

        public async Task DiscardRequestAsync(int id)
        {
            var request = await _cardRenewalRequestRepository.GetByIdAsync(id);

            if (request == null)
            {
                throw new OcudaException("Request does not exist");
            }

            if (request.ProcessedAt.HasValue)
            {
                throw new OcudaException("Request has already been processed");
            }

            var now = _dateTimeProvider.Now;

            var result = new CardRenewalResult
            {
                CardRenewalRequestId = request.Id,
                CreatedAt = now,
                CreatedBy = GetCurrentUserId(),
                IsDiscarded = true
            };
            await _cardRenewalResultRepository.AddAsync(result);


            request.IsDiscarded = true;
            request.ProcessedAt = now;
            _cardRenewalRequestRepository.Update(request);

            await _cardRenewalResultRepository.SaveAsync();
            await _cardRenewalRequestRepository.SaveAsync();
        }

        public async Task<IEnumerable<CardRenewalResponse>> GetAvailableResponsesAsync()
        {
            return await _cardRenewalResponseRepository.GetAvailableAsync();
        }

        public async Task<CardRenewalResponse> GetResponseAsync(int id)
        {
            return await _cardRenewalResponseRepository.FindAsync(id);
        }

        public async Task<CardRenewalResponse> GetResponseTextAsync(int responseId, int languageId)
        {
            var response = await _cardRenewalResponseRepository.FindAsync(responseId);
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

        public async Task<IEnumerable<CardRenewalResponse>> GetResponsesAsync()
        {
            return await _cardRenewalResponseRepository.GetAllAsync();
        }


        public async Task<CardRenewalResult> GetResultForRequestAsync(int requestId)
        {
            return await _cardRenewalResultRepository.GetForRequestAsync(requestId);
        }

        public async Task<ProcessResult> ProcessRequestAsync(int requestId,
            int responseId,
            string responseText,
            string patronName)
        {
            var request = await _cardRenewalRequestRepository
                .GetByIdAsync(requestId);
            if (request.ProcessedAt.HasValue)
            {
                throw new OcudaException("Request has already been processed.");
            }

            var processResult = new ProcessResult();

            var response = await _cardRenewalResponseRepository.FindAsync(responseId);

            if (response.Type == CardRenewalResponse.ResponseType.Accept)
            {
                var renewResult = _polarisHelper.RenewPatronRegistration(
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
                { Keys.CardRenewal.CustomerBarcode, request.Barcode },
                { Keys.CardRenewal.CustomerName, patronName }
            };

            var emailDetails = await _emailService.GetDetailsAsync(response.EmailSetupId.Value,
                language.Name,
                tags,
                responseText);

            emailDetails.ToEmailAddress = request.Email;
            emailDetails.ToName = patronName;

            EmailRecord sentEmail = null;

            try
            {
                sentEmail = await _emailService.SendAsync(emailDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                        "Error sending email setup {EmailSetupId} to {EmailTo}: {ErrorMessage}",
                        response.EmailSetupId.Value,
                        emailDetails.ToEmailAddress,
                        ex.Message);

                throw new OcudaException("Unable to send email.");
            }

            if (sentEmail != null)
            {
                var now = _dateTimeProvider.Now;

                request.ProcessedAt = now;
                _cardRenewalRequestRepository.Update(request);

                var result = new CardRenewalResult
                {
                    CardRenewalRequestId = request.Id,
                    CreatedAt = now,
                    CreatedBy = GetCurrentUserId(),
                    ResponseText = sentEmail.BodyText
                };

                await _cardRenewalResultRepository.AddAsync(result);
                //await _cardRenewalResponseRepository.SaveAsync();
            }


            return processResult;
        }

        public async Task UpdateResponseAsync(CardRenewalResponse response)
        {
            ArgumentNullException.ThrowIfNull(response);

            var currentResponse = await _cardRenewalResponseRepository.FindAsync(response.Id);

            currentResponse.EmailSetupId = response.EmailSetupId;
            currentResponse.Name = response.Name.Trim();

            _cardRenewalResponseRepository.Update(currentResponse);
            await _cardRenewalResponseRepository.SaveAsync();
        }

        public async Task UpdateResponseSortOrderAsync(int id, bool increase)
        {
            var response = await _cardRenewalResponseRepository.FindAsync(id);

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

            var responseInPosition = await _cardRenewalResponseRepository.GetBySortOrderAsync(
                newSortOrder)
                ?? throw new OcudaException("Response is already in the last position.");

            responseInPosition.SortOrder = response.SortOrder;
            response.SortOrder = newSortOrder;

            _cardRenewalResponseRepository.Update(response);
            _cardRenewalResponseRepository.Update(responseInPosition);
            await _cardRenewalResponseRepository.SaveAsync();
        }
    }
}
