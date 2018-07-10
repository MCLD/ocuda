using System;
using Ocuda.Ops.Service;

namespace Ocuda.Ops.Controllers.ServiceFacade
{
    public class Controller
    {
        private readonly SiteSettingService _siteSettingService;

        public SiteSettingService SiteSettingService
        {
            get
            {
                return _siteSettingService;
            }
        }

        public Controller(SiteSettingService siteSettingService)
        {
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }
    }
}
