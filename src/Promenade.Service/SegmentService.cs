using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class SegmentService : BaseService<SegmentService>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISegmentRepository _segmentRepository;
        private readonly ISegmentTextRepository _segmentTextRepository;
        private readonly LanguageService _languageService;

        public SegmentService(ILogger<SegmentService> logger,
            IDateTimeProvider dateTimeProvider,
            IHttpContextAccessor httpContextAccessor,
            ISegmentRepository segmentRepository,
            ISegmentTextRepository segmentTextRepository,
            LanguageService languageService)
            : base(logger, dateTimeProvider)
        {
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _segmentRepository = segmentRepository
                ?? throw new ArgumentNullException(nameof(segmentRepository));
            _segmentTextRepository = segmentTextRepository
                ?? throw new ArgumentNullException(nameof(segmentTextRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

        public async Task<SegmentText> GetSegmentTextBySegmentIdAsync(int segmentId)
        {
            SegmentText segmentText = null;

            var segment = await _segmentRepository.GetActiveAsync(segmentId);

            if (segment != null)
            {

                var currentCultureName = _httpContextAccessor
                    .HttpContext
                    .Features
                    .Get<IRequestCultureFeature>()
                    .RequestCulture
                    .UICulture?
                    .Name;

                if (!string.IsNullOrWhiteSpace(currentCultureName))
                {
                    var currentLangaugeId = await _languageService
                        .GetLanguageIdAsync(currentCultureName);
                    segmentText = await _segmentTextRepository
                        .GetByIdsAsync(currentLangaugeId, segmentId);
                }

                if (segmentText == null)
                {
                    var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();
                    segmentText = await _segmentTextRepository
                        .GetByIdsAsync(defaultLanguageId, segmentId);
                }
            }

            return segmentText;
        }
    }
}
