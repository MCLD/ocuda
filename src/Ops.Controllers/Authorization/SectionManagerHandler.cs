using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Authorization
{
    public class SectionManagerHandler : AuthorizationHandler<SectionManagerRequirement>
    {
        private readonly ILogger _logger;

        public SectionManagerHandler(ILogger<SectionManagerHandler> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            SectionManagerRequirement requirement)
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
                    var resource = context.Resource as Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext;

                    if (resource != null)
                    {
                        var sectionName = resource.RouteData.Values["section"]?.ToString();

                        if (string.IsNullOrEmpty(sectionName))
                        {
                            // default section is managed by site manager
                            context.Fail();
                        }
                        else
                        {
                            var isSectionManager = claims
                                .Where(_ => _.Type == ClaimType.SectionManager 
                                    && _.Value == sectionName.ToLower())
                                .Any();

                            if (isSectionManager)
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
                                _logger.LogWarning($"Access denied for user {username} to manage section {sectionName}");
                                context.Fail();
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"Error decoding section name for authorizaton, resource is wrong type: {context.Resource.GetType()}");
                        throw new Exception("Can't decode section name from context.");
                    }
                }
            }
            else
            {
                context.Fail();
            }
        }
    }
}
