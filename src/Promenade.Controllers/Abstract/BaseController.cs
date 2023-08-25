using System;
using System.Globalization;
using System.Threading.Tasks;
using CommonMark;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Ocuda.i18n;
using Ocuda.i18n.Filter;
using Ocuda.Promenade.Controllers.Filters;
using Ocuda.Promenade.Models.Keys;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.Abstract
{
    [ServiceFilter(typeof(LocalizationFilterAttribute), Order = 10)]
    [ServiceFilter(typeof(LayoutFilter), Order = 20)]
    [MiddlewareFilter(typeof(i18n.Middleware.LocalizationMiddleware))]
    public abstract class BaseController<T> : Controller
    {
        protected readonly IConfiguration _config;
        protected readonly IStringLocalizer<i18n.Resources.Shared> _localizer;
        protected readonly ILogger<T> _logger;
        protected readonly SiteSettingService _siteSettingService;

        protected BaseController(ServiceFacades.Controller<T> context)
        {
            ArgumentNullException.ThrowIfNull(context);

            _logger = context.Logger;
            _config = context.Config;
            _localizer = context.Localizer;
            _siteSettingService = context.SiteSettingService;
        }

        /// <summary>
        /// Property to return the pending info alert, if there is none then return null.
        /// </summary>
        protected string AlertInfo
        {
            get
            {
                return HasAlertInfo
                    ? TempData[TempDataKey.AlertInfo] as string
                    : null;
            }
        }

        /// <summary>
        /// Returns a boolean indicating whether or not there is a pending info alert to show
        /// </summary>
        protected bool HasAlertInfo
        {
            get
            {
                return TempData.ContainsKey(TempDataKey.AlertInfo);
            }
        }

        protected string PageTitle { get; set; }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            var titleSuffix = context?.HttpContext.Items[ItemKey.PageTitleSuffix] as string;

            string pageTitle = !string.IsNullOrWhiteSpace(titleSuffix)
                ? titleSuffix
                : string.Empty;

            if (context?.Controller is BaseController<T> controller
                && !string.IsNullOrWhiteSpace(controller.PageTitle))
            {
                pageTitle = !string.IsNullOrEmpty(titleSuffix)
                    && !titleSuffix.Equals(controller.PageTitle,
                        StringComparison.OrdinalIgnoreCase)
                    ? $"{controller.PageTitle} - {titleSuffix}"
                    : controller.PageTitle;
            }

            ViewData[Utility.Keys.ViewData.Title] = pageTitle;
        }

        /// <summary>
        /// Process a SegmentText to wrap it in the SegmentWrap prefix and suffix and also convert
        /// the text to HTML using the CommonMark converter.
        /// </summary>
        /// <param name="segmentText">The SegmentText as retreived by the database, should include
        /// SegmentWrapPrefix and SegmentWrapSuffix.</param>
        /// <returns>A string containing HTML to display to the user via Html.Raw()</returns>
        protected static string FormatForDisplay(Models.Entities.SegmentText segmentText)
        {
            return segmentText == null
                ? null
                : segmentText.SegmentWrapPrefix?.Trim()
                    + CommonMarkConverter.Convert(segmentText.Text?.Trim())
                    + segmentText.SegmentWrapSuffix?.Trim();
        }

        protected async Task<string> GetCanonicalLinkAsync()
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;
            var isTLS = await _siteSettingService
                .GetSettingBoolAsync(SiteSetting.Site.IsTLS, forceReload);

            var actionLink = new UriBuilder(Url.Action(null, null, null, isTLS
                ? Uri.UriSchemeHttps
                : Uri.UriSchemeHttp));

            // if the current culture is in the URI we'll want to remove it
            actionLink = RemoveCulturePrefix(actionLink, CultureInfo.CurrentCulture.Name);

            if (CultureInfo.CurrentCulture.Name == Culture.DefaultCulture.Name)
            {
                // for the default culture, exclude it from the URI
                return actionLink.Uri.AbsoluteUri;
            }
            else
            {
                // for non-default cultures add properly-cased culture identifier to the URI
                actionLink.Path = CultureInfo.CurrentCulture.Name + actionLink.Path;
                return actionLink.Uri.AbsoluteUri;
            }
        }

        /// <summary>
        /// Set an alert to be shown on the next page load if the page supports showing alerts.
        /// Verify that the redirected page shows alerts before setting this value so that it does
        /// not hang around in TempData indefinitely.
        /// </summary>
        /// <param name="alertMessage">The text of the alert message, raw HTML.</param>
        protected void SetAlertInfo(string alertMessage)
        {
            TempData[TempDataKey.AlertInfo] = alertMessage;
        }

        /// <summary>
        /// If the path of the supplied <see cref="System.UriBuilder">UriBuilder</see> starts with
        /// a slash, the specified cultureIdentifier, and then a slash, strip out one slash and
        /// the cultureIdentifier and return the resulting UriBuilder.
        /// </summary>
        /// <param name="uriBuilder">A UriBuilder containing the page URI of interest</param>
        /// <param name="cultureIdentifier">The culture identifier to remove frmo the URI</param>
        /// <returns>A well-formed UriBuilder with the culture identifier removed</returns>
        private static UriBuilder RemoveCulturePrefix(UriBuilder uriBuilder,
            string cultureIdentifier)
        {
            var cultureInUrl = $"/{cultureIdentifier}/";
            if (uriBuilder.Path.Length > cultureInUrl.Length - 1
                && uriBuilder.Path.StartsWith(cultureInUrl, StringComparison.OrdinalIgnoreCase))
            {
                uriBuilder.Path = uriBuilder.Path[(cultureInUrl.Length - 1)..];
            }
            return uriBuilder;
        }
    }
}