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

        public async Task<SegmentText> GetSegmentTextBySegmentAndLanguageId(int segmentId, int? languageId)
        {
            if (languageId==null) {
                var currentCultureName = _httpContextAccessor
                    .HttpContext
                    .Features
                    .Get<IRequestCultureFeature>()
                    .RequestCulture
                    .UICulture?
                    .Name;
                if (!string.IsNullOrWhiteSpace(currentCultureName))
                {
                    languageId = await _languageService
                        .GetLanguageIdAsync(currentCultureName);
                }
            }
            var segment = await _segmentRepository.FindAsync(segmentId);
            if (segment?.IsActive == true &&
                (segment?.StartDate == null || _dateTimeProvider.Now > segment?.StartDate) &&
                (segment?.EndDate == null || _dateTimeProvider.Now < segment?.EndDate))
            {
                return _segmentTextRepository.GetSegmentTextBySegmentAndLanguageId(segment.Id, languageId.Value);
            }
            return null;
        }
    }
}
