using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;

namespace Ocuda.Ops.Service
{
    public class InitialSetupService : IInitialSetupService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IConfiguration _config;
        private readonly ISiteSettingPromService _siteSettingPromService;
        private readonly ISiteSettingService _siteSettingService;
        private readonly IUserManagementService _userManagementService;

        public InitialSetupService(IAuthorizationService authorizationService,
            IConfiguration configuration,
            ISiteSettingPromService siteSettingPromService,
            ISiteSettingService siteSettingService,
            IUserManagementService userManagementService)
        {
            ArgumentNullException.ThrowIfNull(authorizationService);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(siteSettingPromService);
            ArgumentNullException.ThrowIfNull(siteSettingService);
            ArgumentNullException.ThrowIfNull(userManagementService);

            _authorizationService = authorizationService;
            _config = configuration;
            _siteSettingPromService = siteSettingPromService;
            _siteSettingService = siteSettingService;
            _userManagementService = userManagementService;
        }

        public async Task VerifyInitialSetupAsync()
        {
            // ensure the sysadmin user exists
            var sysadminUser = await _userManagementService.EnsureSysadminUserAsync();
            await _siteSettingService.EnsureSiteSettingsExistAsync(sysadminUser.Id);
            await _siteSettingPromService.EnsureSiteSettingsExistAsync();

            var siteManagerGroup = _config[Utility.Keys.Configuration.OpsSiteManagerGroup];
            if (!string.IsNullOrEmpty(siteManagerGroup))
            {
                await _authorizationService.EnsureSiteManagerGroupAsync(sysadminUser.Id,
                    siteManagerGroup);
            }
        }
    }
}