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
            try
            {
                var languageService = _scope.ServiceProvider.GetRequiredService<LanguageService>();
                await languageService.SyncLanguagesAsync();
            }
            catch (InvalidOperationException ioex)
            {
                _log.LogCritical(ioex,
                    "Error acquiring LanguageService to sync languages in database: {ErrorMessage}",
                    ioex.Message);
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex,
                    "Error syncing available languages with database: {ErrorMessage}",
                    ex.Message);
                throw;
            }
        }
    }
}
