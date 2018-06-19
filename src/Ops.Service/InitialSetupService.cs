using System;
using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Service
{
    public class InitialSetupService
    {
        private readonly ILogger _logger;
        private readonly UserService _userService;

        public InitialSetupService(ILogger<InitialSetupService> logger,
            SectionService sectionService,
            UserService userService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userService = userService
                ?? throw new ArgumentNullException(nameof(userService));
        }

        public void VerifyInitialSetup()
        {
            // ensure the sysadmin user exists
            _userService.EnsureSysadminUser();
        }
    }
}
