using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers
{
    public class UserContextProvider : IUserContextProvider
    {
        private readonly ILogger<UserContextProvider> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextProvider(ILogger<UserContextProvider> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public string UserClaim(ClaimsPrincipal user, string claimType)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var claim = user
                .Claims
                .Where(_ => _.Type == claimType);

            switch (claim.Count())
            {
                case 0:
                    return null;

                case 1:
                    return claim.First().Value;

                default:
                    string userId = user.Claims
                        .FirstOrDefault(_ => _.Type == ClaimType.UserId)?.Value ?? "Unknown";
                    var distinct = claim.Select(_ => _.Value).Distinct();
                    if (distinct.Count() > 1)
                    {
                        throw new OcudaException(string.Format("User {0} has multiple {1} claims: {2}",
                            userId,
                            claimType,
                            string.Join(",", claim.Select(_ => _.Value))));
                    }
                    else
                    {
                        return claim.First().Value;
                    }
            }
        }

        public List<string> UserClaims(ClaimsPrincipal user, string claimType)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return user
                .Claims?
                .Where(_ => _.Type == claimType)?
                .Select(_ => _.Value)?
                .Distinct()?
                .ToList();
        }
    }
}