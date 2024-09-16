using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ocuda.i18n.Filter
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public sealed class LocalizationFilterAttribute : Attribute, IAsyncResourceFilter
    {
        private const string CultureRouteKey = "culture";

        private readonly IOptions<RequestLocalizationOptions> _l10nOptions;
        private readonly ILogger<RequestLocalizationOptions> _logger;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1019:Define accessors for attribute arguments",
            Justification = "Arguments are for internal use only.")]
        public LocalizationFilterAttribute(ILogger<RequestLocalizationOptions> logger,
            IOptions<RequestLocalizationOptions> l10nOptions)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(l10nOptions);

            _logger = logger;
            _l10nOptions = l10nOptions;
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            if (_l10nOptions.Value?.SupportedCultures.Count > 1)
            {
                string requestedCulture = context.RouteData.Values.ContainsKey(CultureRouteKey)
                    ? context.RouteData.Values[CultureRouteKey] as string
                    : null;

                var cultureCookieSet = context.HttpContext
                    .Request
                    .Cookies
                    .ContainsKey(CookieRequestCultureProvider.DefaultCookieName);

                if (!string.IsNullOrEmpty(requestedCulture))
                {
                    // set to requested culture
                    if (requestedCulture == Culture.DefaultName)
                    {
                        context.HttpContext
                            .Response
                            .Cookies
                            .Delete(CookieRequestCultureProvider.DefaultCookieName);
                    }
                    else
                    {
                        context.HttpContext.Response.Cookies.Append(
                            CookieRequestCultureProvider.DefaultCookieName,
                            CookieRequestCultureProvider
                                .MakeCookieValue(new RequestCulture(requestedCulture)),
                            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(14) });
                    }
                }
                else
                {
                    // no request, if default, clear the cookie
                    if (cultureCookieSet && CultureInfo.CurrentCulture.Name == Culture.DefaultName)
                    {
                        context.HttpContext
                            .Response
                            .Cookies
                            .Delete(CookieRequestCultureProvider.DefaultCookieName);
                    }
                }

                var uriBuilder = new UriBuilder(context.HttpContext.Request.Scheme,
                    context.HttpContext.Request.Host.Host);

                if (context.HttpContext.Request.Host.Port.HasValue)
                {
                    uriBuilder.Port = context.HttpContext.Request.Host.Port.Value;
                }

                var uriPathBuilder = new StringBuilder();

                if (context.HttpContext.Request.Path.HasValue)
                {
                    string path = context.HttpContext.Request.Path.Value;
                    var cultureCheck = $"/{CultureInfo.CurrentCulture.Name}";
                    var requestCultureCheck = !string.IsNullOrEmpty(requestedCulture)
                        ? $"/{requestedCulture}"
                        : null;

                    if (path.StartsWith(cultureCheck))
                    {
                        uriPathBuilder.Append(path,
                            cultureCheck.Length,
                            path.Length - cultureCheck.Length);
                    }
                    else if (!string.IsNullOrEmpty(requestCultureCheck)
                       && path.StartsWith(requestCultureCheck))
                    {
                        uriPathBuilder.Append(path,
                            requestCultureCheck.Length,
                            path.Length - requestCultureCheck.Length);
                    }
                    else
                    {
                        uriPathBuilder.Append(path);
                    }
                }

                string uriPath = uriPathBuilder.ToString();

                if (context.HttpContext.Request.QueryString.HasValue)
                {
                    uriPathBuilder.Append(context.HttpContext.Request.QueryString);
                }

                var cultureList = new Dictionary<string, string>();

                Uri builtPath;
                try
                {
                    builtPath = new Uri(uriBuilder.Uri, Culture.DefaultName + uriPath);
                }
                catch (UriFormatException ufex)
                {
                    _logger.LogCritical("Error composing URI from {URI} and {Path}: {ErrorMessage}",
                        uriBuilder.Uri,
                        Culture.DefaultName + uriPath,
                        ufex.Message);
                    throw;
                }

                var cultureHrefLang = new Dictionary<string, string>
                {
                    { "x-default", builtPath.AbsoluteUri }
                };

                foreach (var culture in _l10nOptions.Value.SupportedCultures)
                {
                    var text = culture.Parent != null
                        ? culture.Parent.NativeName
                        : culture.NativeName;

                    builtPath = new Uri(uriBuilder.Uri, culture.Name + uriPath);
                    cultureList.Add(text, builtPath.AbsoluteUri);

                    if (!cultureHrefLang.ContainsKey(culture.Name))
                    {
                        cultureHrefLang.Add(culture.Name, builtPath.AbsoluteUri);

                        if (culture.Parent != null
                            && !cultureHrefLang.ContainsKey(culture.Parent.Name))
                        {
                            cultureHrefLang.Add(culture.Parent.Name, builtPath.AbsoluteUri);
                        }
                    }
                }

                context.HttpContext.Items[LocalizationItemKey.HrefLang] = cultureHrefLang;
                context.HttpContext.Items[LocalizationItemKey.L10n] = cultureList;
            }

            await next();
        }
    }
}