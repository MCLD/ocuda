using System;
using Ocuda.Ops.Service;

namespace Ocuda.Ops.Controllers.ServiceFacade
{
    public class Controller
    {
        public readonly SiteSettingService SiteSettingService;

        public Controller(SiteSettingService siteSettingService)
        {
            SiteSettingService = siteSettingService 
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }
    }
}
