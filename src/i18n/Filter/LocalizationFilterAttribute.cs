using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace Ocuda.i18n.Filter
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class LocalizationFilterAttribute : Attribute, IAsyncResourceFilter
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
            var httpContext = context.HttpContext;

            var currentCulture = _cultureContextProvider.GetCurrentCulture();
            httpContext.Items[LocalizationItemKey.ISOLanguageName]
                = currentCulture.TwoLetterISOLanguageName;

            if (_l10nOptions.Value?.SupportedCultures.Count > 1)
            {
                var cookieCulture = httpContext
                    .Request
                    .Cookies[CookieRequestCultureProvider.DefaultCookieName];

                if (currentCulture.Name == Culture.DefaultName)
                {
                    if (cookieCulture != null)
                    {
                        httpContext
                            .Response
                            .Cookies
                            .Delete(CookieRequestCultureProvider.DefaultCookieName);
                    }
                }
                else
                {
                    // no cookie or new culture selected, reset cookie
                    httpContext.Response.Cookies.Append(
                        CookieRequestCultureProvider.DefaultCookieName,
                        CookieRequestCultureProvider
                            .MakeCookieValue(new RequestCulture(currentCulture.Name)),
                        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(14) }
                    );
                }

                // generate list for drop-down
                var cultureList = new List<SelectListItem>();
                var cultureHrefLang = new Dictionary<string, string>
                {
                    { "x-default", Culture.DefaultName }
                };
                foreach (var culture in _l10nOptions.Value.SupportedCultures)
                {
                    var text = culture.Parent != null
                        ? culture.Parent.NativeName
                        : culture.NativeName;
                    cultureList.Add(new SelectListItem(text, culture.Name));
                    if (!cultureHrefLang.Keys.Contains(culture.Name))
                    {
                        cultureHrefLang.Add(culture.Name, culture.Name);
                        if (culture.Parent != null
                            && !cultureHrefLang.Keys.Contains(culture.Parent.Name))
                        {
                            cultureHrefLang.Add(culture.Parent.Name, culture.Parent.Name);
                        }
                    }
                }
                httpContext.Items[LocalizationItemKey.HrefLang] = cultureHrefLang;
                httpContext.Items[LocalizationItemKey.L10n] = cultureList.OrderBy(_ => _.Text);
            }

            await next();
        }
    }
}
