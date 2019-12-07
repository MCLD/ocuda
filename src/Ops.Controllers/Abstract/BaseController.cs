using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Abstract
{
    [ServiceFilter(typeof(AuthenticationFilterAttribute))]
    [ServiceFilter(typeof(ExternalResourceFilterAttribute))]
    [ServiceFilter(typeof(NavigationFilterAttribute))]
    [ServiceFilter(typeof(UserFilterAttribute))]
    public abstract class BaseController<T> : Controller
    {
        protected readonly ILogger _logger;
        protected readonly ISiteSettingService _siteSettingService;
        protected readonly IUserContextProvider _userContextProvider;

        protected BaseController(ServiceFacades.Controller<T> context)
        {
            _logger = context.Logger;
            _siteSettingService = context.SiteSettingService;
            _userContextProvider = context.UserContextProvider;
        }

        protected string AlertDanger
        {
            set
            {
                TempData[TempDataKey.AlertDanger] = value;
            }
        }

        protected string AlertWarning
        {
            set
            {
                TempData[TempDataKey.AlertWarning] = value;
            }
        }

        protected string AlertInfo
        {
            set
            {
                TempData[TempDataKey.AlertInfo] = value;
            }
        }

        protected string AlertSuccess
        {
            set
            {
                TempData[TempDataKey.AlertSuccess] = value;
            }
        }

        protected ClaimsPrincipal AuthUser
        {
            get
            {
                return HttpContext.User;
            }
        }

        private string Fa(string iconName, string iconStyle = "fa")
        {
            return $"<span class=\"{iconStyle} fa-{iconName}\" aria-hidden=\"true\"></span>";
        }

        protected void ShowAlertDanger(string message, string details = null)
        {
            AlertDanger = $"{Fa("exclamation-triangle")} {message}{details}";
        }

        protected void ShowAlertWarning(string message, string details = null)
        {
            AlertWarning = $"{Fa("exclamation-circle")} {message}{details}";
        }

        protected void ShowAlertSuccess(string message, string faIconName = null)
        {
            if (!string.IsNullOrEmpty(faIconName))
            {
                AlertSuccess = $"{Fa(faIconName)} {message}";
            }
            else
            {
                AlertSuccess = $"{Fa("thumbs-up", "far")} {message}";
            }
        }

        protected void ShowAlertInfo(string message, string faIconName = null)
        {
            if (!string.IsNullOrEmpty(faIconName))
            {
                AlertInfo = $"{Fa(faIconName)} {message}";
            }
            else
            {
                AlertInfo = $"{Fa("check-circle")} {message}";
            }
        }

        protected string CurrentUsername
        {
            get
            {
                return HttpContext.User.Identity.Name;
            }
        }

        protected string UserClaim(string claimType)
        {
            return _userContextProvider.UserClaim(AuthUser, claimType);
        }

        protected List<string> UserClaims(string claimType)
        {
            return _userContextProvider.UserClaims(AuthUser, claimType);
        }

        protected int CurrentUserId
        {
            get
            {
                var userIdString = HttpContext.User.Claims
                    .FirstOrDefault(_ => _.Type == ClaimType.UserId)?
                    .Value;
                if (int.TryParse(userIdString, out int userId))
                {
                    return userId;
                }
                else
                {
                    // TODO is this the right approach here?
                    return -1;
                }
            }
        }
    }
}
