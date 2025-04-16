using System.Collections.Generic;
using System.Security.Claims;

namespace Ocuda.Ops.Service.Abstract
{
    public interface IUserContextProvider
    {
        string UserClaim(ClaimsPrincipal user, string claimType);

        List<string> UserClaims(ClaimsPrincipal user, string claimType);
    }
}