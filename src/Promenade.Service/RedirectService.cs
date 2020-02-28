using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class RedirectService : BaseService<RedirectService>
    {
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;
        private readonly IUrlRedirectAccessRepository _urlRedirectAccessRepository;
        private readonly IUrlRedirectRepository _urlRedirectRepository;

        public RedirectService(ILogger<RedirectService> logger,
            IDateTimeProvider dateTimeProvider,
            IConfiguration config,
            IDistributedCache cache,
            IUrlRedirectAccessRepository urlRedirectAccessRepository,
            IUrlRedirectRepository urlRedirectRepository)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _urlRedirectAccessRepository = urlRedirectAccessRepository
                ?? throw new ArgumentNullException(nameof(urlRedirectAccessRepository));
            _urlRedirectRepository = urlRedirectRepository
                ?? throw new ArgumentNullException(nameof(urlRedirectRepository));
        }

        public async Task<UrlRedirect> GetUrlRedirectByPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string cacheHours = _config[Utility.Keys.Configuration.PromenadeCacheRedirectsHours];

            var cacheKey = string.IsNullOrEmpty(cacheHours)
                ? null
                : string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromRedirectPath,
                    path);

            UrlRedirect redirect = null;

            if (cacheKey != null)
            {
                string cachedRedirect = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedRedirect))
                {
                    try
                    {
                        redirect = JsonSerializer.Deserialize<UrlRedirect>(cachedRedirect);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex,
                            "Error deserializing redirect path {path} from cache ({CacheKey}): {ErrorMessage}",
                            path,
                            cacheKey,
                            ex.Message);
                    }
                }
            }

            if (redirect == null)
            {
                redirect = await _urlRedirectRepository.GetRedirectIdPathAsync(path.TrimEnd('/'));

                if (cacheKey != null && redirect != null)
                {
                    if (!int.TryParse(cacheHours, out int cacheTime))
                    {
                        cacheTime = 1;
                    }

                    string redirectToCache = JsonSerializer.Serialize(redirect);
                    await _cache.SetStringAsync(cacheKey,
                        redirectToCache,
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(cacheTime)
                        });
                }
            }

            if (redirect != null)
            {
                await _urlRedirectAccessRepository.AddAccessLogAsync(redirect.Id);
            }

            return redirect;
        }
    }
}
