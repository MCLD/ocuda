using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Ocuda.i18n.Middleware
{
    public class LocalizationMiddleware
    {
        public void Configure(
            IApplicationBuilder app,
            IOptions<RequestLocalizationOptions> l10nOptions)
        {
            app.UseRequestLocalization(l10nOptions.Value);
        }
    }
}
