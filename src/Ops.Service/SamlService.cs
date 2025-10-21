using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

        public static string GetRedirectLink(IdentityProvider identityProvider)
        {
            ArgumentNullException.ThrowIfNull(identityProvider);

            return new AuthRequest(
                identityProvider.EntityId,
                identityProvider.AssertionConsumerLink)
            .GetRedirectUrl(identityProvider.PostLink);
        }

        public IdentityResponse ValidateLogin(IdentityProvider identityProvider,
            string samlResponse)
        {
            _logger.LogDebug("Creating data protection provider");
            var protector = _dataProtectionProvider
                .CreateProtector($"IdentityProvider.{identityProvider.Id}");

            try
            {
                _logger.LogDebug("Unprotecting SAML certificate");
                var cert = protector.Unprotect(identityProvider.EncryptedCertificate);

                if (string.IsNullOrEmpty(cert))
                {
                    _logger.LogError("Unprotected SAML certificate is empty");
                    throw new OcudaException("Unprotected certificate is empty.");
                }

                _logger.LogDebug("Reading and decoding SAML data");
                var response = new Response(cert, samlResponse);

                var identityResponse = new IdentityResponse()
                {
                    IsValid = response.IsValid()
                };

                if (identityResponse.IsValid)
                {
                    _logger.LogDebug("SAML authorized user: {NameId}", response.GetNameID());
                    identityResponse.Email = response.GetEmail();
                    identityResponse.FirstName = response.GetFirstName();
                    identityResponse.Id = response.GetNameID();
                    identityResponse.LastName = response.GetLastName();
                }

                _logger.LogDebug("Returning SAML authoriztion");
                return identityResponse;
            }
            catch (System.Security.Cryptography.CryptographicException cex {
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