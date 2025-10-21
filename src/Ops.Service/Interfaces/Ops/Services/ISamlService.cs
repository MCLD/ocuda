using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ISamlService
    {
        abstract static string GetRedirectLink(IdentityProvider identityProvider);

        IdentityResponse ValidateLogin(IdentityProvider identityProvider, string samlResponse);
    }
}