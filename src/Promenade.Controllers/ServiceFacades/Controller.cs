using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Ocuda.i18n.Resources;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.ServiceFacades
{
    public class Controller<T>
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly IStringLocalizer<Shared> _sharedLocalizer;
        private readonly SiteSettingService _siteSettingService;

        public ILogger Logger
        {
            get
            {
                return _logger;
            }
        }

        public IConfiguration Config
        {
            get
            {
                return _config;
            }
        }

        public IStringLocalizer<Shared> SharedLocalizer
        {
            get
            {
                return _sharedLocalizer;
            }
        }

        public SiteSettingService SiteSettingService
        {
            get
            {
                return _siteSettingService;
            }
        }

        public Controller(ILogger<T> logger, IConfiguration config,
            IStringLocalizer<Shared> sharedLocalizer,
            SiteSettingService siteSettingService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _sharedLocalizer = sharedLocalizer
                ?? throw new ArgumentNullException(nameof(sharedLocalizer));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }
    }
}
