using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models.Abstract;
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

        protected string AlertWarning
        {
            set
            {
                TempData[TempDataKey.AlertWarning] = value;
            }
        }

        protected ClaimsPrincipal AuthUser
        {
            get
            {
                return HttpContext.User;
            }
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

        protected string CurrentUsername
        {
            get
            {
                return HttpContext.User.Identity.Name;
            }
        }

        protected async Task<bool> HasAppPermissionAsync(
            IPermissionGroupService permissionGroupService,
            string applicationPermission)
        {
            if (!string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)))
            {
                return true;
            }

            var permissionClaims = UserClaims(ClaimType.PermissionId);

            if (permissionClaims.Count > 0)
            {
                if (permissionGroupService == null)
                {
                    throw new ArgumentNullException(nameof(permissionGroupService));
                }
                var needPermissionGroups = await permissionGroupService
                    .GetApplicationPermissionGroupsAsync(applicationPermission);

                if (needPermissionGroups?.Count > 0)
                {
                    var needAPermission = needPermissionGroups
                        .Select(_ => _.Id.ToString(CultureInfo.InvariantCulture));

                    return needAPermission.Intersect(permissionClaims).Any();
                }
            }

            return false;
        }

        protected async Task<bool> HasPermissionAsync<TPermissonGroupMappingBase>(
            IPermissionGroupService permissionGroupService,
            int itemId) where TPermissonGroupMappingBase : PermissionGroupMappingBase
        {
            if (!string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)))
            {
                return true;
            }
            else
            {
                var permissionClaims = UserClaims(ClaimType.PermissionId);
                if (permissionClaims.Count > 0)
                {
                    if (permissionGroupService == null)
                    {
                        throw new ArgumentNullException(nameof(permissionGroupService));
                    }
                    var permissionGroups = await permissionGroupService
                        .GetPermissionsAsync<TPermissonGroupMappingBase>(itemId);
                    var permissionGroupsStrings = permissionGroups
                        .Select(_ => _.PermissionGroupId.ToString(CultureInfo.InvariantCulture));

                    return permissionClaims.Any(_ => permissionGroupsStrings.Contains(_));
                }
                return false;
            }
        }

        protected IActionResult RedirectToUnauthorized()
        {
            return RedirectToAction(nameof(HomeController.Unauthorized),
               HomeController.Name,
               new { area = "", returnUrl = new Uri(Request.GetDisplayUrl()) });
        }

        protected void ShowAlertDanger(string message, string details = null)
        {
            AlertDanger = $"{Fa("exclamation-triangle")} {message}{details}";
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

        protected void ShowAlertWarning(string message, string details = null)
        {
            AlertWarning = $"{Fa("exclamation-circle")} {message}{details}";
        }

        protected string UserClaim(string claimType)
        {
            return _userContextProvider.UserClaim(AuthUser, claimType);
        }

        protected List<string> UserClaims(string claimType)
        {
            return _userContextProvider.UserClaims(AuthUser, claimType);
        }

        private string Fa(string iconName, string iconStyle = "fa")
        {
            return $"<span class=\"{iconStyle} fa-{iconName}\" aria-hidden=\"true\"></span>";
        }
    }
}