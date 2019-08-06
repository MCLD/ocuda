using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class RedirectService : BaseService<RedirectService>
    {
        private readonly IUrlRedirectAccessRepository _urlRedirectAccessRepository;
        private readonly IUrlRedirectRepository _urlRedirectRepository;

        public RedirectService(ILogger<RedirectService> logger,
            IDateTimeProvider dateTimeProvider,
            IUrlRedirectAccessRepository urlRedirectAccessRepository,
            IUrlRedirectRepository urlRedirectRepository)
            : base(logger, dateTimeProvider)
        {
            _urlRedirectAccessRepository = urlRedirectAccessRepository
                ?? throw new ArgumentNullException(nameof(urlRedirectAccessRepository));
            _urlRedirectRepository = urlRedirectRepository
                ?? throw new ArgumentNullException(nameof(urlRedirectRepository));
        }

        public async Task<UrlRedirect> GetUrlRedirectByPathAsync(string path,
            Dictionary<string, string> queryParams)
        {
            var redirects = await _urlRedirectRepository.GetRedirectsByPathAsync(path.TrimEnd('/'));
            UrlRedirect redirect = null;

            if (queryParams.Count > 0)
            {
                var queryRedirects = redirects.Where(_ => !string.IsNullOrWhiteSpace(_.QueryKey)
                    && queryParams.Any(q => q.Key == _.QueryKey && q.Value == _.QueryValue));

                if (queryRedirects.Count() == 1)
                {
                    redirect = queryRedirects.Single();
                }
            }

            if (redirect == null)
            {
                var nonQueryRedirects = redirects.Where(_ => string.IsNullOrWhiteSpace(_.QueryKey));

                if (nonQueryRedirects.Count() == 1)
                {
                    redirect = nonQueryRedirects.Single();
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
