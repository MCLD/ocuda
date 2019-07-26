using System;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.ServiceFacades
{
    public class Controller<T>
    {
        private readonly ILogger _logger;
        private readonly SiteSettingService _siteSettingService;

        public ILogger Logger
        {
            get
            {
                return _logger;
            }
        }

        public SiteSettingService SiteSettingService
        {
            get
            {
                return _siteSettingService;
            }
        }

        public Controller(SiteSettingService siteSettingService, ILogger<T> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }
    }
}
