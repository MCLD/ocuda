using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class SegmentService : BaseService<SegmentService>, ISegmentService
    {
        private readonly IEmediaGroupRepository _emediaGroupRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IScheduleRequestSubjectRepository _scheduleRequestSubjectRepository;
        private readonly ISegmentRepository _segmentRepository;
        private readonly ISegmentTextRepository _segmentTextRepository;

        public SegmentService(ILogger<SegmentService> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmediaGroupRepository emediaGroupRepository,
            ILocationRepository locationRepository,
            IScheduleRequestSubjectRepository scheduleRequestSubjectRepository,
            ISegmentRepository segmentRepository,
            ISegmentTextRepository segmentTextRepository) : base(logger, httpContextAccessor)
        {
            _emediaGroupRepository = emediaGroupRepository
                ?? throw new ArgumentNullException(nameof(emediaGroupRepository));
            _locationRepository = locationRepository
                ?? throw new ArgumentNullException(nameof(locationRepository));
            _scheduleRequestSubjectRepository = scheduleRequestSubjectRepository
                ?? throw new ArgumentNullException(nameof(scheduleRequestSubjectRepository));
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

        public async Task<Segment> GetByIdAsync(int segmentId)
        {
            return await _segmentRepository.FindAsync(segmentId);
        }

        public async Task<SegmentText> GetBySegmentAndLanguageAsync(int segmentId, int languageId)
        {
            return await _segmentTextRepository.GetBySegmentAndLanguageAsync(segmentId, languageId);
        }

        public async Task<ICollection<string>> GetSegmentLanguagesByIdAsync(int id)
        {
            return await _segmentTextRepository.GetUsedLanguageNamesBySegmentId(id);
        }

        public async Task<Segment> CreateAsync(Segment segment)
        {
            segment.Name = segment.Name?.Trim();

            await _segmentRepository.AddAsync(segment);
            await _segmentRepository.SaveAsync();
            return segment;
        }

        public async Task<Segment> EditAsync(Segment segment)
        {
            var currentSegment = await _segmentRepository.FindAsync(segment.Id);

            currentSegment.Name = segment.Name.Trim();
            currentSegment.IsActive = segment.IsActive;
            currentSegment.StartDate = segment.StartDate;
            currentSegment.EndDate = segment.EndDate;

            _segmentRepository.Update(currentSegment);
            await _segmentRepository.SaveAsync();
            return segment;
        }

        public async Task DeleteAsync(int id)
        {
            var inUseBy = await GetSegmentInUseByAsync(id);
            if (inUseBy.Count > 0)
            {
                var ocudaException = new OcudaException("Segment is in use by the following");
                ocudaException.Data[OcudaExceptionData.SegmentInUseBy] = inUseBy;

                throw ocudaException;
            }

            var segment = await _segmentRepository.FindAsync(id);
            var segmentTexts = await _segmentTextRepository.GetBySegmentIdAsync(segment.Id);

            _segmentTextRepository.RemoveRange(segmentTexts);
            _segmentRepository.Remove(segment);
            await _segmentRepository.SaveAsync();
        }

        public async Task CreateSegmentTextAsync(SegmentText segmentText)
        {
            segmentText.Text = segmentText.Text?.Trim();
            segmentText.Header = segmentText.Header?.Trim();

            await _segmentTextRepository.AddAsync(segmentText);
            await _segmentTextRepository.SaveAsync();
        }

        public async Task EditSegmentTextAsync(SegmentText segmentText)
        {
            var currentSegmentText = await _segmentTextRepository
                .GetBySegmentAndLanguageAsync(segmentText.SegmentId, segmentText.LanguageId);
            currentSegmentText.Text = segmentText.Text?.Trim();
            currentSegmentText.Header = segmentText.Header?.Trim();

            _segmentTextRepository.Update(currentSegmentText);
            await _segmentTextRepository.SaveAsync();
        }

        public async Task DeleteSegmentTextAsync(SegmentText segmentText)
        {
            _segmentTextRepository.Remove(segmentText);
            await _segmentTextRepository.SaveAsync();
        }

        private async Task<ICollection<string>> GetSegmentInUseByAsync(int id)
        {
            var inUseBy = new List<string>();

            var emediaGroups = await _emediaGroupRepository.GetUsingSegmentAsync(id);
            foreach (var emediaGroup  in emediaGroups)
            {
                inUseBy.Add($"Emedia Group: {emediaGroup.Name}");
            }

            var locations = await _locationRepository.GetUsingSegmentAsync(id);
            foreach (var location in locations)
            {
                inUseBy.Add($"Location: {location.Name}");
            }

            var scheduleRequestSubjects = await _scheduleRequestSubjectRepository
                .GetUsingSegmentAsync(id);
            foreach (var scheduleRequestSubject in scheduleRequestSubjects)
            {
                inUseBy.Add($"Schedule Request Subject: {scheduleRequestSubject.Subject}");
            }

            return inUseBy;
        }
    }
}
