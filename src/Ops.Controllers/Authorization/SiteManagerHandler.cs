using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Key;

namespace Ocuda.Ops.Controllers.Authorization
{
    public class SiteManagerHandler : AuthorizationHandler<SiteManagerRequirement>
    {
        private readonly ILogger _logger;

        public SiteManagerHandler(ILogger<SiteManagerHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            SiteManagerRequirement requirement)
        {
            var claims = context.User.Claims;

            if (claims != null && claims.Count() > 0)
            {
                var isSiteManager = claims
                    .Where(_ => _.Type == ClaimType.SiteManager)
                    .Any();

                if (isSiteManager)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    var username = context.User
                        .Claims
                        .Where(_ => _.Type == ClaimType.Username)
                        .FirstOrDefault()?
                        .Value ?? "Unknown";
                    _logger.LogWarning($"Access denied for user {username} as site manager");
                    context.Fail();
                }
            }
            else
            {
                context.Fail();
            }
        }
    }
}
