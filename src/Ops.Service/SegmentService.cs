using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class SegmentService : BaseService<SegmentService>, ISegmentService
    {
        private readonly ISegmentRepository _segmentRepository;
        private readonly ISegmentTextRepository _segmentTextRepository;

        public SegmentService(ILogger<SegmentService> logger,
            IHttpContextAccessor httpContextAccessor,
            ISegmentRepository segmentRepository,
            ISegmentTextRepository segmentTextRepository) : base(logger, httpContextAccessor)
        {
            _segmentRepository = segmentRepository
                ?? throw new ArgumentNullException(nameof(segmentRepository));
            _segmentTextRepository = segmentTextRepository
                            ?? throw new ArgumentNullException(nameof(segmentTextRepository));
        }

        public async Task<ICollection<Segment>> GetActiveSegmentsAsync()
        {
            return await _segmentRepository.GetAllActiveSegmentsAsync();
        }

        public async Task<DataWithCount<ICollection<Segment>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _segmentRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Segment> GetSegmentById(int segmentId)
        {
            return await _segmentRepository.FindAsync(segmentId);
        }

        public SegmentText GetBySegmentIdAndLanguage(int segmentId, int languageId)
        {
            return _segmentTextRepository.GetSegmentTextBySegmentId(segmentId, languageId);
        }

        public async Task<ICollection<string>> GetSegmentLanguagesByIdAsync(int id)
        {
            return await _segmentTextRepository.GetUsedLanguageNamesBySegmentId(id);
        }

        public Segment FindSegmentByName(string name)
        {
            return _segmentRepository.FindSegmentByName(name);
        }

        public async Task AddSegment(Segment segment)
        {
            segment.Name = segment.Name.Trim();
            if (! await _segmentRepository.IsDuplicateNameAsync(segment))
            {
                await _segmentRepository.AddAsync(segment);
                await _segmentRepository.SaveAsync();
            }
        }

        public async Task EditSegment(Segment segment)
        {
            if (!await _segmentRepository.IsDuplicateNameAsync(segment))
            {
                var currentSegment = await _segmentRepository.FindAsync(segment.Id);
                currentSegment.Name = segment.Name.Trim();
                currentSegment.IsActive = segment.IsActive;
                currentSegment.StartDate = segment.StartDate;
                currentSegment.EndDate = segment.EndDate;

                _segmentRepository.Update(currentSegment);
                await _segmentRepository.SaveAsync();
            }
            else
            {
                throw new OcudaException($"Segment name: {segment.Name} already exists");
            }
        }

        public async Task AddSegmentText(SegmentText segmentText)
        {
            segmentText.Text = segmentText.Text?.Trim();
            segmentText.Header = segmentText.Header?.Trim();
            await _segmentTextRepository.AddAsync(segmentText);
            await _segmentTextRepository.SaveAsync();
        }

        public async Task EditSegmentText(SegmentText segmentText)
        {
            var currentSegmentText = _segmentTextRepository.GetSegmentTextBySegmentId(segmentText.SegmentId,segmentText.LanguageId);
            currentSegmentText.Text = segmentText.Text?.Trim();
            currentSegmentText.Header = segmentText.Header?.Trim();

            _segmentTextRepository.Update(currentSegmentText);
            await _segmentTextRepository.SaveAsync();
        }

        public async Task DeleteSegmentText(SegmentText segmentText)
        {
            _segmentTextRepository.Remove(segmentText);
            await _segmentTextRepository.SaveAsync();
        }
    }
}
