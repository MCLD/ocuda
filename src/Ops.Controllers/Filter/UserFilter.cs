using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Controllers.Filter
{
    public class UserFilter : Attribute, IAsyncResourceFilter
    {
        private readonly ILogger<UserFilter> _logger;
        private readonly IUserService _userService;

        public UserFilter(ILogger<UserFilter> logger, IUserService userService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            var usernameClaim = context.HttpContext.User
                .Claims
                .Where(_ => _.Type == Key.ClaimType.Username)
                .FirstOrDefault();

            if (usernameClaim != null)
            {
                var user = await _userService.LookupUser(usernameClaim.Value);
                if (user != null)
                {
                    context.HttpContext.Items[ItemKey.Nickname] = user.Nickname ?? user.Username;
                }
            }

            await next();
        }
    }
}
