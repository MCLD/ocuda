using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;

namespace Ocuda.Ops.Service
{
    public class LanguageService : ILanguageService
    {
        private readonly ILogger<LanguageService> _logger;
        private readonly IOptions<RequestLocalizationOptions> _l10nOptions;
        private readonly ILanguageRepository _languageRepository;

        public LanguageService(ILogger<LanguageService> logger,
            IOptions<RequestLocalizationOptions> l10nOptions,
            ILanguageRepository languageRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _l10nOptions = l10nOptions ?? throw new ArgumentNullException(nameof(l10nOptions));
            _languageRepository = languageRepository
                ?? throw new ArgumentNullException(nameof(languageRepository));
        }
    }
}
