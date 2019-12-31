using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Exceptions;
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
            SectionManagerRequirement requirement)
        {
            var claims = context.User.Claims;

            if (claims?.Any() == true)
            {
                var isSiteManager = claims
                    .Any(_ => _.Type == ClaimType.SiteManager);

                if (isSiteManager)
                {
                    context.Succeed(requirement);
                }
                else
                {
                    if (context.Resource
                        is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext resource)
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
                                .Any(_ => _.Type == ClaimType.SectionManager
                                    && _.Value == sectionName.ToLower());

                            if (isSectionManager)
                            {
                                context.Succeed(requirement);
                            }
                            else
                            {
                                var username = context.User
                                    .Claims
                                    .FirstOrDefault(_ => _.Type == ClaimType.Username)?
                                    .Value ?? "Unknown";
                                _logger.LogWarning($"Access denied for user {username} to manage section {sectionName}");
                                context.Fail();
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError($"Error decoding section name for authorizaton, resource is wrong type: {context.Resource.GetType()}");
                        throw new OcudaException("Can't decode section name from context.");
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
