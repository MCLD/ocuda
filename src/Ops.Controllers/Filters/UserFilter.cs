using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Filters
{
    public class UserFilterAttribute : Attribute, IAsyncResourceFilter
    {
        private readonly ILogger<UserFilterAttribute> _logger;
        private readonly ISectionService _sectionService;
        private readonly IUserService _userService;

        public UserFilterAttribute(ILogger<UserFilterAttribute> logger,
            ISectionService sectionService,
            IUserService userService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            var usernameClaim = context.HttpContext.User
                .Claims
                .FirstOrDefault(_ => _.Type == ClaimType.Username);

            if (usernameClaim != null)
            {
                var user = await _userService.LookupUserAsync(usernameClaim.Value);
                if (user != null)
                {
                    context.HttpContext.Items[ItemKey.Nickname] = user.Nickname ?? user.Username;

                    var sections = await _sectionService.GetAllAsync();
                    if (sections?.Count > 0)
                    {
                        bool userIsSupervisor = false;
                        try
                        {
                            userIsSupervisor = await _userService.IsSupervisor(user.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Could not determine if userid {UserId} is a supervisor: {ErrorMessage}",
                                user.Id,
                                ex.Message);
                        }
                        if (!userIsSupervisor)
                        {
                            sections = sections.Where(_ => _.SupervisorsOnly != true).ToList();
                        }
                    }
                    context.HttpContext.Items[ItemKey.Sections] = sections;
                }
            }

            await next();
        }
    }
}