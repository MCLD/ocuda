using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Service
{
    public class InitialSetupService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly AuthorizationService _authorizationService;
        private readonly SectionService _sectionService;
        private readonly UserService _userService;

        public InitialSetupService(ILogger<InitialSetupService> logger,
            IConfiguration configuration,
            AuthorizationService authorizationService,
            SectionService sectionService,
            UserService userService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _authorizationService = authorizationService 
                ?? throw new ArgumentNullException(nameof(authorizationService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task VerifyInitialSetupAsync()
        {
            // ensure the sysadmin user exists
            var sysadminUser = await _userService.EnsureSysadminUserAsync();
            await _sectionService.EnsureDefaultSectionAsync(sysadminUser.Id);

            var siteManagerGroup = _config[Utility.Keys.Configuration.OpsSiteManagerGroup];
            if (!string.IsNullOrEmpty(siteManagerGroup))
            {
                await _authorizationService.EnsureSiteManagerGroupAsync(sysadminUser.Id, 
                    siteManagerGroup);
            }
        }
    }
}
