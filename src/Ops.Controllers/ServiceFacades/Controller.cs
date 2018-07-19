using System;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Controllers.ServiceFacades
{
    public class Controller<T>
    {
        private readonly ILogger _logger;
        private readonly ISiteSettingService _siteSettingService;

        public ILogger Logger
        {
            get
            {
                return _logger;
            }
        }

        public ISiteSettingService SiteSettingService
        {
            get
            {
                return _siteSettingService;
            }
        }

        public Controller(ISiteSettingService siteSettingService, ILogger<T> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }
    }
}
