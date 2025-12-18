using System;
using Microsoft.Extensions.Primitives;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Saml;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ISamlService
    {
        public static string GenerateRedirectLink(IdentityProvider provider)
        {
            return GenerateRedirectLink(provider, null);
        }

        public static string GenerateRedirectLink(IdentityProvider provider, string relayState)
        {
            ArgumentNullException.ThrowIfNull(provider);
            return new AuthRequest(provider.EntityId, provider.AssertionConsumerLink)
                .GetRedirectUrl(provider.EndpointLink, relayState);
        }

        IdentityResponse ValidateLogin(IdentityProvider provider, StringValues samlResponse);
    }
}