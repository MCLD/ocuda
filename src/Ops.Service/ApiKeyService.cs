using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class ApiKeyService : BaseService<ApiKeyService>, IApiKeyService
    {
        private readonly IApiKeyRepository _apiKeyRepository;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ApiKeyService(ILogger<ApiKeyService> logger,
            IHttpContextAccessor httpContextAccessor,
            IApiKeyRepository apiKeyRepository,
            IDateTimeProvider dateTimeProvider) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(apiKeyRepository);
            ArgumentNullException.ThrowIfNull(dateTimeProvider);

            _apiKeyRepository = apiKeyRepository;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<string> CreateAsync(ApiKeyType keyType,
            int representsUserId,
            DateTime? endDate)
        {
            var code = Guid.NewGuid();

            await _apiKeyRepository.AddAsync(new ApiKey
            {
                CreatedAt = _dateTimeProvider.Now,
                CreatedBy = GetCurrentUserId(),
                EndDate = endDate,
                Identifier = code.ToString()[..8],
                Key = SHA256.HashData(code.ToByteArray()),
                KeyType = keyType,
                RepresentsUserId = representsUserId
            });
            await _apiKeyRepository.SaveAsync();
            return code.ToString();
        }

        public async Task DeleteAsync(int apiKeyId)
        {
            var apiKey = await _apiKeyRepository.FindAsync(apiKeyId);
            _apiKeyRepository.Remove(apiKey);
            await _apiKeyRepository.SaveAsync();
        }

        public async Task<ApiKey> FindAsync(string apiKey)
        {
            ArgumentNullException.ThrowIfNull(apiKey);

            Guid? code;
            try
            {
                code = new Guid(apiKey);
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                _logger.LogError(ex,
                    "API request with invalid key {ApiKey}: {ErrorMessage}",
                    apiKey,
                    ex.Message);
                throw new OcudaException("Unable to parse API key", ex);
            }

            if (!code.HasValue)
            {
                _logger.LogError("API request with unparsable key {ApiKey}",
                    apiKey);
                throw new OcudaException("Unable to parse API key");
            }

            return await _apiKeyRepository
                .FindByKeyAsync(SHA256.HashData(code.Value.ToByteArray()));
        }

        public async Task<CollectionWithCount<ApiKey>> PageAsync(BaseFilter filter)
        {
            return await _apiKeyRepository.PageAsync(filter);
        }
    }
}