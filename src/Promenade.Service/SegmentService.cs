using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class SegmentService : BaseService<SegmentService>
    {
        private readonly ISegmentRepository _segmentRepository;
        private readonly ISegmentTextRepository _segmentTextRepository;

        public SegmentService(ILogger<SegmentService> logger,
            IDateTimeProvider dateTimeProvider,
            ISegmentRepository segmentRepository,
            ISegmentTextRepository segmentTextRepository)
            : base(logger, dateTimeProvider)
        {
            _segmentRepository = segmentRepository
                ?? throw new ArgumentNullException(nameof(segmentRepository));
            _segmentTextRepository = segmentTextRepository
                ?? throw new ArgumentNullException(nameof(segmentTextRepository));
        }

        public async Task<Segment> GetSegmentById(int segmentId)
        {
            var segment = await _segmentRepository.FindAsync(segmentId);
            segment.SegmentText = _segmentTextRepository.GetSegmentTextBySegmentId(segment.Id);
            return segment;
        }

        public async Task<SegmentText> GetSegmentTextBySegmentAndLanguageId(int segmentId, int languageId)
        {
            var segment = await _segmentRepository.FindAsync(segmentId);
            if (segment != null && segment.IsActive)
            {
                if (segment.StartDate != null && segment.EndDate != null)
                {
                    var today = DateTime.Now;
                    if (today > segment.StartDate && today < segment.EndDate)
                    {
                        return _segmentTextRepository.GetSegmentTextBySgmentAndLanguageId(segment.Id, languageId);
                    }
                }
                else
                {
                    return _segmentTextRepository.GetSegmentTextBySgmentAndLanguageId(segment.Id, languageId);
                }
            }
            return null;
        }
    }
}
