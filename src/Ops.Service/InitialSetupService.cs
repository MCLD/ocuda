using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Service
{
    public class InitialSetupService
    {
        private readonly ILogger _logger;
        private readonly SectionService _sectionService;
        private readonly UserService _userService;

        public InitialSetupService(ILogger<InitialSetupService> logger,
            SectionService sectionService,
            UserService userService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        }
    }
}
