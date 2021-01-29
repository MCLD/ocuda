using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Ocuda.i18n.Filter
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public sealed class LocalizationFilterAttribute : Attribute, IAsyncResourceFilter
    {
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

                string requestedCulture = null;

                if (context.HttpContext.Request.Query.ContainsKey("culture"))
                {
                    requestedCulture = context.HttpContext.Request.Query["culture"][0];
                }

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

                Dictionary<string, StringValues> queryString = null;
                if (context.HttpContext.Request.QueryString.HasValue)
                {
                    queryString
                        = QueryHelpers.ParseQuery(context.HttpContext.Request.QueryString.Value);
                    queryString.Remove("culture");
                }

                var currentUri = string.Format(CultureInfo.InvariantCulture,
                    "{0}://{1}{2}{3}",
                    context.HttpContext.Request.Scheme,
                    context.HttpContext.Request.Host,
                    context.HttpContext.Request.Path.HasValue
                        ? context.HttpContext.Request.Path.Value : "",
                    queryString != null ? new QueryBuilder(queryString).ToQueryString() : "");

                // generate list for drop-down
                var cultureList = new Dictionary<string, string>();
                var cultureHrefLang = new Dictionary<string, string>
                {
                    { "x-default",
                        QueryHelpers.AddQueryString(currentUri, "culture", Culture.DefaultName) }
                };

                foreach (var culture in _l10nOptions.Value.SupportedCultures)
                {
                    var text = culture.Parent != null
                        ? culture.Parent.NativeName
                        : culture.NativeName;

                    cultureList.Add(text,
                        QueryHelpers.AddQueryString(currentUri, "culture", culture.Name));

                    if (!cultureHrefLang.ContainsKey(culture.Name))
                    {
                        cultureHrefLang.Add(culture.Name,
                            QueryHelpers.AddQueryString(currentUri, "culture", culture.Name));

                        if (culture.Parent != null
                            && !cultureHrefLang.ContainsKey(culture.Parent.Name))
                        {
                            cultureHrefLang.Add(culture.Parent.Name,
                                QueryHelpers.AddQueryString(currentUri,
                                    "culture",
                                    culture.Parent.Name));
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
