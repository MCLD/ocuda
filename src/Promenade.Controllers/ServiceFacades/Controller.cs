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
            IStringLocalizer<Shared> localizer,
            SiteSettingService siteSettingService)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            SiteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        public IConfiguration Config { get; }

        public IStringLocalizer<Shared> Localizer { get; }
        public ILogger<T> Logger { get; }
        public SiteSettingService SiteSettingService { get; }
    }
}