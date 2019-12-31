using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class SiteAlertService : BaseService<SiteAlertService>
    {
        private readonly ISiteAlertRepository _siteAlertRepository;

        public SiteAlertService(ILogger<SiteAlertService> logger,
            IDateTimeProvider dateTimeProvider,
            ISiteAlertRepository siteAlertRepository)
            : base(logger, dateTimeProvider)
        {
            _siteAlertRepository = siteAlertRepository
                ?? throw new ArgumentNullException(nameof(siteAlertRepository));
        }

        public async Task<ICollection<SiteAlert>> GetCurrentSiteAlertsAsync()
        {
            return await _siteAlertRepository.GetCurrentSiteAlertsAsync();
        }
    }
}
