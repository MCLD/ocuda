using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Web
{
    public class Web
    {
        private readonly ILogger<Web> _log;
        private readonly IServiceScope _scope;

        public Web(IServiceScope scope)
        {
            _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            _log = scope.ServiceProvider.GetRequiredService<ILogger<Web>>();
        }

        public async Task InitalizeAsync()
        {
            int stage = 10;
            try
            {
                var languageService = _scope.ServiceProvider.GetRequiredService<LanguageService>();
                await languageService.SyncLanguagesAsync();
            }
            catch (Exception ex)
            {
                bool critical = false;
                string errorText;
                switch (stage)
                {
                    case 10:
                        errorText = "Error syncing available languages with database: {Message}";
                        break;
                    default:
                        errorText = "Unknown error during application startup: {Message}";
                        break;
                }

                if (critical)
                {
                    _log.LogCritical(ex, errorText, ex.Message);
                    throw;
                }
                else
                {
                    _log.LogCritical(ex, errorText, ex.Message);
                }
            }
        }
    }
}
