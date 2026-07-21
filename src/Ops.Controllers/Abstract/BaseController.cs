using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models.Abstract;
using Ocuda.Ops.Models.Definitions.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Abstract
{
    [Authorize]
    public abstract class BaseController<T>(Controller<T> context)
        : BaseUnauthenticatedController<T>(context)
    {
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

        private IUserContextProvider UserContextProvider { get; } = context.UserContextProvider;

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
            int itemId)
            where TPermissonGroupMappingBase : PermissionGroupMappingBase
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

        /// <summary>
        /// Take the passed in <see cref="ReportDefinition"/> and use the provided
        /// <see cref="IPermissionGroupService"/> to populate the IsPermittedImport and
        /// IsPermittedView settings based on the current user's rights.
        /// </summary>
        /// <param name="permissionGroupService">A valid <see cref="IPermissionGroupService"/>.
        /// </param>
        /// <param name="report">The <see cref="ReportDefinition"/> to look up.</param>
        /// <returns>The populated <see cref="ReportDefinition"/> with IsPermittedImport and
        /// IsPermittedView set correctly.</returns>
        protected async Task<ReportDefinition> PopulatePermissionsAsync(
            IPermissionGroupService permissionGroupService,
            ReportDefinition report)
        {
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(report);

            if (await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.ImportAllReports) || IsSiteManager())
            {
                report.IsPermittedImport = true;
                report.IsPermittedView = true;
            }
            else
            {
                var perms = await permissionGroupService
                    .GetPermissionsAsync<PermissionGroupReporting>(report.InternalId);

                report.IsPermittedImport = perms?.Any(_ => _.CanImport) == true;
                report.IsPermittedView = perms?.Count > 0;
            }

            return report;
        }

        /// <summary>
        /// Take the passed in <see cref="IEnumerable{ReportDefinition}"/> list and use the provided
        /// <see cref="IPermissionGroupService"/> to populate the IsPermittedImport and
        /// IsPermittedView settings based on the current user's rights.
        /// </summary>
        /// <param name="permissionGroupService">A valid <see cref="IPermissionGroupService"/>.
        /// </param>
        /// <param name="reports">The collection of reports to look up.</param>
        /// <returns>The populated <see cref="IEnumerable{ReportDefinition}"/> with
        /// IsPermittedImport and IsPermittedView set correctly.</returns>
        protected async Task<IEnumerable<ReportDefinition>> PopulatePermissionsAsync(
            IPermissionGroupService permissionGroupService,
            IEnumerable<ReportDefinition> reports)
        {
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(reports);

            ICollection<ReportDefinition> permissionedReports = [];

            foreach (var pendingReport in reports)
            {
                permissionedReports.Add(await PopulatePermissionsAsync(permissionGroupService,
                    pendingReport));
            }

            return permissionedReports;
        }

        protected string UserClaim(string claimType)
        {
            return UserContextProvider.UserClaim(AuthUser, claimType);
        }

        protected IList<string> UserClaims(string claimType)
        {
            return UserContextProvider.UserClaims(AuthUser, claimType);
        }

        protected IEnumerable<FileThumbnail> PopulateThumbnailLinks(
            string fileLibrarySlug,
            string sectionSlug,
            IEnumerable<FileThumbnail> thumbnails)
        {
            foreach (var thumbnail in thumbnails)
            {
                thumbnail.Link = Url.Action(
                    nameof(HomeController.GetThumbnail),
                    HomeController.Name,
                    new
                    {
                        area = string.Empty,
                        fileLibrarySlug,
                        sectionSlug,
                        thumbnailId = thumbnail.Id,
                    });
            }

            return thumbnails;
        }
    }
}
