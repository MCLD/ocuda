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
        public Controller(ILogger<T> logger,
            IConfiguration config,
            IStringLocalizer<Shared> sharedLocalizer,
            SiteSettingService siteSettingService)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            SharedLocalizer = sharedLocalizer
                ?? throw new ArgumentNullException(nameof(sharedLocalizer));
            SiteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        public IConfiguration Config { get; }

        public ILogger<T> Logger { get; }

        public IStringLocalizer<Shared> SharedLocalizer { get; }

        public SiteSettingService SiteSettingService { get; }
    }
}