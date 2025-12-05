using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Saml;

namespace Ocuda.Ops.Service
{
    public class SamlService : BaseService<SamlService>, ISamlService
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public SamlService(ILogger<SamlService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDataProtectionProvider dataProtectionProvider)
                : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(dataProtectionProvider);

            _dataProtectionProvider = dataProtectionProvider;
        }

        public IdentityResponse ValidateLogin(IdentityProvider provider, StringValues samlResponse)
        {
            ArgumentNullException.ThrowIfNull(provider);

            var protector = _dataProtectionProvider
                .CreateProtector($"IdentityProvider.{provider.Id}");

            try
            {
                var cert = protector.Unprotect(provider.EncryptedCertificate);

                if (string.IsNullOrEmpty(cert))
                {
                    _logger.LogError("Unprotected SAML certificate is empty");
                    throw new OcudaException("Unprotected certificate is empty.");
                }

                var response = new Response(cert, samlResponse);

                var identityResponse = new IdentityResponse()
                {
                    IsValid = response.IsValid()
                };

                if (identityResponse.IsValid)
                {
                    identityResponse.Email = response.GetEmail();
                    identityResponse.FirstName = response.GetFirstName();
                    identityResponse.UserId = response.GetNameID();
                    identityResponse.LastName = response.GetLastName();
                }

                return identityResponse;
            }
            catch (System.Security.Cryptography.CryptographicException cex)
            {
                // couldn't unprotect the certificate
                _logger.LogError(cex,
                    "Unable to unprotect SAML certificate: {ErrorMessage}",
                    cex.Message);
                throw new OcudaException("Unable to access SAML certificate, please re-upload",
                    cex);
            }
        }
    }
}