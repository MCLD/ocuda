using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class InitialSetupService : IInitialSetupService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISectionService _sectionService;
        private readonly IUserService _userService;

        public InitialSetupService(ILogger<InitialSetupService> logger,
            IConfiguration configuration,
            IAuthorizationService authorizationService,
            ISectionService sectionService,
            IUserService userService)
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
