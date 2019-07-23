using System;
using System.Collections.Generic;
using System.Text;
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

        public async Task<UrlRedirect> GetUrlRedirectByPathAsync(string path)
        {
            var redirect = await _urlRedirectRepository.GetRedirectByPathAsync(path);

            if (redirect != null)
            {
                await _urlRedirectAccessRepository.AddAccessLogAsync(redirect.Id);
            }

            return redirect;
        }
    }
}
