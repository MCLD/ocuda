using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
    public class IdentityProviderService
        : BaseService<IdentityProviderService>, IIdentityProviderService
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIdentityProviderRepository _identityProviderRepository;

        public IdentityProviderService(ILogger<IdentityProviderService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDataProtectionProvider dataProtectionProvider,
            IDateTimeProvider dateTimeProvider,
            IIdentityProviderRepository identityProviderRepository)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(dataProtectionProvider);
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(identityProviderRepository);

            _dataProtectionProvider = dataProtectionProvider;
            _dateTimeProvider = dateTimeProvider;
            _identityProviderRepository = identityProviderRepository;
        }

        public async Task AddProviderAsync(IdentityProvider provider, string certificate)
        {
            ArgumentNullException.ThrowIfNull(certificate);
            ArgumentNullException.ThrowIfNull(provider);

            provider.Active = true;
            provider.CreatedAt = DateTime.Now;
            provider.EncryptedCertificate = null;
            provider.Id = default;

            _logger.LogDebug("Adding provider");
            await _identityProviderRepository.AddAsync(provider);
            await _identityProviderRepository.SaveAsync();

            _logger.LogDebug("Provider inserted with id {ProviderId}", provider.Id);

            _logger.LogDebug("Creating data protection provider");
            var protector = _dataProtectionProvider
                .CreateProtector($"IdentityProvider.{provider.Id}");

            try
            {
                _logger.LogDebug("Protecting SAML certificate");
                provider.EncryptedCertificate = protector.Protect(certificate);

                _logger.LogDebug("Saving protected SAML certificate");
                _identityProviderRepository.Update(provider);
                await _identityProviderRepository.SaveAsync();
            }
            catch (CryptographicException cex)
            {
                _logger.LogError(cex,
                    "Unable to protect SAML certificate: {ErrorMessage}",
                    cex.Message);
                throw new OcudaException("Unable to save SAML certificate, please re-upload", cex);
            }
        }

        public async Task<DataWithCount<IEnumerable<IdentityProvider>>>
            GetProvidersAsync(BaseFilter filter)
        {
            return new DataWithCount<IEnumerable<IdentityProvider>>
            {
                Count = await _identityProviderRepository.CountAsync(filter),
                Data = await _identityProviderRepository.PageAsync(filter)
            };
        }
    }
}