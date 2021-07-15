using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Ocuda.i18n.Filter
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public sealed class LocalizationFilterAttribute : Attribute, IAsyncResourceFilter
    {
        private const string CultureRouteKey = "culture";

        private readonly CultureContextProvider _cultureContextProvider;
        private readonly IOptions<RequestLocalizationOptions> _l10nOptions;

        public LocalizationFilterAttribute(CultureContextProvider cultureContextProvider,
            IOptions<RequestLocalizationOptions> l10nOptions)
        {
            _cultureContextProvider = cultureContextProvider
                ?? throw new ArgumentNullException(nameof(cultureContextProvider));
            _l10nOptions = l10nOptions
                ?? throw new ArgumentNullException(nameof(l10nOptions));
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            if (_l10nOptions.Value?.SupportedCultures.Count > 1)
            {
                var currentCulture = _cultureContextProvider.GetCurrentCulture();

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
                    if (cultureCookieSet && currentCulture.Name == Culture.DefaultName)
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
                    var cultureCheck = $"/{currentCulture.Name}";
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

                var builtPath = new Uri(uriBuilder.Uri, Culture.DefaultName + uriPath);
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

                context.HttpContext.Items[LocalizationItemKey.CurrentCulture] = currentCulture;

                context.HttpContext.Items[LocalizationItemKey.HrefLang] = cultureHrefLang;
                context.HttpContext.Items[LocalizationItemKey.L10n] = cultureList;
            }

            await next();
        }
    }
}