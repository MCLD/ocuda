using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Ocuda.i18n
{
    public class CultureContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CultureContextProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public CultureInfo GetCurrentCulture()
        {
            return _httpContextAccessor
                .HttpContext
                .Features
                .Get<IRequestCultureFeature>()
                .RequestCulture
                .UICulture;
        }
    }
}
