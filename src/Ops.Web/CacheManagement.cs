using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Web
{
    public class CacheManagement
    {
        private readonly ILogger<CacheManagement> _log;
        private readonly IServiceProvider _serviceProvider;

        public CacheManagement(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _log = serviceProvider.GetRequiredService<ILogger<CacheManagement>>();
        }

        public async Task StartupClearAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var cache = scope.ServiceProvider.GetRequiredService<IOcudaCache>();
                await cache.RemoveAsync(Utility.Keys.Cache.OpsSections);
                _log.LogInformation("Cache clear for {CacheItem} upon startup", "sections");
            }
            catch (InvalidOperationException ioex)
            {
                _log.LogCritical(ioex,
                    "Error acquiring OcudaCache to clear cache upon startup: {ErrorMessage}",
                    ioex.Message);
            }
            catch (Exception ex)
            {
                _log.LogCritical(ex,
                    "Error clearing cache upon startup: {ErrorMessage}",
                    ex.Message);
                throw;
            }
        }
    }
}