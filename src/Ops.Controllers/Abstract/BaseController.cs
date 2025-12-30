using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models.Abstract;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Abstract
{
    [Authorize]
    public abstract class BaseController<T> : BaseUnauthenticatedController<T>
    {
        protected readonly IUserContextProvider _userContextProvider;

        protected BaseController(Controller<T> context) : base(context)
        {
            _userContextProvider = context.UserContextProvider;
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
                // TODO is this the right approach here? possibly throw
                return int.TryParse(userIdString, out int userId) ? userId : -1;
            }
        }

        protected string CurrentUsername
        {
            get
            {
                return HttpContext.User.Identity.Name;
            }
        }

        protected async Task<Uri> GetBaseUriAsync(ISiteSettingService siteSettingService)
        {
            ArgumentNullException.ThrowIfNull(siteSettingService);
            return new Uri(await siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.UserInterface.BaseIntranetLink));
        }

        protected async Task<bool> HasAppPermissionAsync(
            IPermissionGroupService permissionGroupService,
            string applicationPermission)
        {
            if (!string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                && !string.IsNullOrEmpty(applicationPermission)
                && !applicationPermission.Equals(ApplicationPermission.MultiUserAccount,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var permissionClaims = UserClaims(ClaimType.PermissionId);

            if (permissionClaims.Count > 0)
            {
                ArgumentNullException.ThrowIfNull(permissionGroupService);
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
                    ArgumentNullException.ThrowIfNull(permissionGroupService);
                    var permissionGroups = await permissionGroupService
                        .GetPermissionsAsync<TPermissonGroupMappingBase>(itemId);
                    var permissionGroupsStrings = permissionGroups
                        .Select(_ => _.PermissionGroupId.ToString(CultureInfo.InvariantCulture));

                    return permissionClaims.Any(_ => permissionGroupsStrings.Contains(_));
                }
                return false;
            }
        }

        protected bool IsSiteManager()
        {
            return !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager));
        }

        protected string UserClaim(string claimType)
        {
            return _userContextProvider.UserClaim(AuthUser, claimType);
        }

        protected IList<string> UserClaims(string claimType)
        {
            return _userContextProvider.UserClaims(AuthUser, claimType);
        }
    }
}