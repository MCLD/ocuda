using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Serilog.Context;

namespace Ocuda.Ops.Controllers.Filters
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public sealed class UserFilterAttribute : Attribute, IAsyncResourceFilter
    {
        public UserFilterAttribute(ILogger<UserFilterAttribute> logger,
            ISectionService sectionService,
            IUserService userService)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(sectionService);
            ArgumentNullException.ThrowIfNull(userService);

            Logger = logger;
            SectionService = sectionService;
            UserService = userService;
        }

        public ILogger<UserFilterAttribute> Logger { get; }

        public ISectionService SectionService { get; }

        public IUserService UserService { get; }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            User user = null;
            var usernameClaim = context.HttpContext.User
                .Claims
                .FirstOrDefault(_ => _.Type == ClaimType.Username);

            // no claim = not authenticat4ed, redirect

            if (usernameClaim != default)
            {
                user = await UserService.LookupUserAsync(usernameClaim.Value);
                if (user != null)
                {
                    context.HttpContext.Items[ItemKey.Nickname] = user.Nickname ?? user.Username;
                }
            }

            var sections = await SectionService.GetAllAsync();
            if (sections?.Count > 0)
            {
                bool userIsSupervisor = false;
                try
                {
                    userIsSupervisor = user != null && await UserService.IsSupervisor(user.Id);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Could not determine if userid {UserId} is a supervisor: {ErrorMessage}",
                        user.Id,
                        ex.Message);
                }
                if (!userIsSupervisor)
                {
                    sections = sections.Where(_ => !_.SupervisorsOnly).ToList();
                }
            }
            context.HttpContext.Items[ItemKey.Sections] = sections;

            using (LogContext.PushProperty(Utility.Logging.Enrichment.HttpMethod,
                context.HttpContext.Request.Method))
            using (LogContext.PushProperty(Utility.Logging.Enrichment.HttpReferer,
                context.HttpContext.Request.Headers.Referer))
            using (LogContext.PushProperty(Utility.Logging.Enrichment.RouteAction,
                context.RouteData?.Values["action"]))
            using (LogContext.PushProperty(Utility.Logging.Enrichment.RouteArea,
                context.RouteData?.Values["area"]))
            using (LogContext.PushProperty(Utility.Logging.Enrichment.RouteController,
                context.RouteData?.Values["controller"]))
            using (LogContext.PushProperty(Utility.Logging.Enrichment.RouteId,
                context.RouteData?.Values["id"]))
            {
                if (user != null)
                {
                    using (LogContext.PushProperty(Utility.Logging.Enrichment.UserId, user?.Id))
                    using (LogContext.PushProperty(Utility.Logging.Enrichment.Username,
                        user?.Username))
                    {
                        await next();
                    }
                }
                else
                {
                    await next();
                }
            }
        }
    }
}