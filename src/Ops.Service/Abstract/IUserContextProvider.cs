using System.Threading.Tasks;
using System.Security.Claims;
using System.Globalization;
using System.Collections.Generic;

namespace Ocuda.Ops.Service.Abstract
{
    public interface IUserContextProvider
    {
        string UserClaim(ClaimsPrincipal user, string claimType);
        List<string> UserClaims(ClaimsPrincipal user, string claimType);
    }
}
