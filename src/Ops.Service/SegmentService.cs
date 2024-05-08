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
        private readonly IPodcastItemsRepository _podcastItemsRepository;
        private readonly IScheduleRequestSubjectRepository _scheduleRequestSubjectRepository;
        private readonly ISegmentRepository _segmentRepository;
        private readonly ISegmentTextRepository _segmentTextRepository;

        public SegmentService(ILogger<SegmentService> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmediaGroupRepository emediaGroupRepository,
            ILocationRepository locationRepository,
            IPodcastItemsRepository podcastItemsRepository,
            IScheduleRequestSubjectRepository scheduleRequestSubjectRepository,
            ISegmentRepository segmentRepository,
            ISegmentTextRepository segmentTextRepository) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(emediaGroupRepository);
            ArgumentNullException.ThrowIfNull(locationRepository);
            ArgumentNullException.ThrowIfNull(podcastItemsRepository);
            ArgumentNullException.ThrowIfNull(scheduleRequestSubjectRepository);
            ArgumentNullException.ThrowIfNull(segmentRepository);
            ArgumentNullException.ThrowIfNull(segmentTextRepository);

            _emediaGroupRepository = emediaGroupRepository;
            _locationRepository = locationRepository;
            _podcastItemsRepository = podcastItemsRepository;
            _scheduleRequestSubjectRepository = scheduleRequestSubjectRepository;
            _segmentRepository = segmentRepository;
            _segmentTextRepository = segmentTextRepository;
        }

        public async Task<Segment> CreateAsync(Segment segment)
        {
            segment = await CreateNoSaveAsync(segment);

            await _segmentRepository.SaveAsync();
            return segment;
        }

        public async Task<Segment> CreateNoSaveAsync(Segment segment)
        {
            ArgumentNullException.ThrowIfNull(segment);

            segment.Name = segment.Name?.Trim();

            await _segmentRepository.AddAsync(segment);
            return segment;
        }

        public async Task CreateSegmentTextAsync(SegmentText segmentText)
        {
            ArgumentNullException.ThrowIfNull(segmentText);

            segmentText.Text = segmentText.Text?.Trim();
            segmentText.Header = segmentText.Header?.Trim();

            await _segmentTextRepository.AddAsync(segmentText);
            await _segmentTextRepository.SaveAsync();
        }

        public async Task DeleteAsync(int segmentId)
        {
            var inUseBy = await GetSegmentInUseByAsync(segmentId);
            if (inUseBy.Count > 0)
            {
                var ocudaException = new OcudaException("Segment is in use by the following");
                ocudaException.Data[OcudaExceptionData.SegmentInUseBy] = inUseBy;

                throw ocudaException;
            }

            await DeleteNoSaveAsync(segmentId);

            await _segmentRepository.SaveAsync();
        }

        public async Task DeleteNoSaveAsync(int id)
        {
            var segment = await _segmentRepository.FindAsync(id);
            var segmentTexts = await _segmentTextRepository.GetBySegmentIdAsync(id);

            _segmentTextRepository.RemoveRange(segmentTexts);
            _segmentRepository.Remove(segment);
        }

        public async Task DeleteSegmentTextAsync(SegmentText segmentText)
        {
            _segmentTextRepository.Remove(segmentText);
            await _segmentTextRepository.SaveAsync();
        }

        public async Task DeleteWithTextsAlreadyVerifiedAsync(int segmentId)
        {
            await DeleteNoSaveAsync(segmentId);
            await _segmentRepository.SaveAsync();
        }

        public async Task<Segment> EditAsync(Segment segment)
        {
            ArgumentNullException.ThrowIfNull(segment);

            var currentSegment = await _segmentRepository.FindAsync(segment.Id);

            currentSegment.Name = segment.Name?.Trim();
            currentSegment.IsActive = segment.IsActive;
            currentSegment.StartDate = segment.StartDate;
            currentSegment.EndDate = segment.EndDate;

            _segmentRepository.Update(currentSegment);
            await _segmentRepository.SaveAsync();
            return segment;
        }

        public async Task EditSegmentTextAsync(SegmentText segmentText)
        {
            ArgumentNullException.ThrowIfNull(segmentText);

            var currentSegmentText = await _segmentTextRepository
                .GetBySegmentAndLanguageAsync(segmentText.SegmentId, segmentText.LanguageId);
            currentSegmentText.Text = segmentText.Text?.Trim();
            currentSegmentText.Header = segmentText.Header?.Trim();

            _segmentTextRepository.Update(currentSegmentText);
            await _segmentTextRepository.SaveAsync();
        }

        public async Task<ICollection<Segment>> GetActiveSegmentsAsync()
        {
            return await _segmentRepository.GetAllActiveSegmentsAsync();
        }

        public async Task<Segment> GetByIdAsync(int segmentId)
        {
            return await _segmentRepository.FindAsync(segmentId);
        }

        public async Task<SegmentText> GetBySegmentAndLanguageAsync(int segmentId, int languageId)
        {
            return await _segmentTextRepository.GetBySegmentAndLanguageAsync(segmentId, languageId);
        }

        public async Task<IDictionary<int, string>> GetNamesByIdsAsync(IEnumerable<int> ids)
        {
            return await _segmentRepository.GetNamesByIdsAsync(ids);
        }

        public async Task<int?> GetPageHeaderIdForSegmentAsync(int id)
        {
            return await _segmentRepository.GetPageHeaderIdForSegmentAsync(id);
        }

        public async Task<int?> GetPageLayoutIdForSegmentAsync(int id)
        {
            return await _segmentRepository.GetPageLayoutIdForSegmentAsync(id);
        }

        public async Task<DataWithCount<ICollection<Segment>>> GetPaginatedListAsync(
                                                    BaseFilter filter)
        {
            return await _segmentRepository.GetPaginatedListAsync(filter);
        }

        public async Task<ICollection<string>> GetSegmentLanguagesByIdAsync(int id)
        {
            return await _segmentTextRepository.GetUsedLanguageNamesBySegmentId(id);
        }

        public async Task UpdateWrapAsync(int segmentId, int? segmentWrapId)
        {
            await _segmentRepository.UpdateWrapAsync(segmentId, segmentWrapId);
        }

        private async Task<ICollection<string>> GetSegmentInUseByAsync(int id)
        {
            // LocationFeature is handled in LocationService
            var inUseBy = new List<string>();

            var emediaGroup = await _emediaGroupRepository.GetUsingSegmentAsync(id);
            if (emediaGroup != null)
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

            var episode = await _podcastItemsRepository.GetUsingSegmentAsync(id);
            if (episode != null)
            {
                inUseBy.Add($"Podcast Episode: {episode.Title}");
            }

            return inUseBy;
        }
    }
}