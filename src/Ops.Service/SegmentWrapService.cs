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
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class SegmentWrapService : BaseService<SegmentWrapService>, ISegmentWrapService
    {
        private readonly ISegmentRepository _segmentRepository;
        private readonly ISegmentWrapRepository _segmentWrapRepository;

        public SegmentWrapService(ILogger<SegmentWrapService> logger,
            IHttpContextAccessor httpContextAccessor,
            ISegmentRepository segmentRepository,
            ISegmentWrapRepository segmentWrapRepository) : base(logger, httpContextAccessor)
        {
            _segmentRepository = segmentRepository
                ?? throw new ArgumentNullException(nameof(segmentRepository));
            _segmentWrapRepository = segmentWrapRepository
                ?? throw new ArgumentNullException(nameof(segmentWrapRepository));
        }

        public async Task AddAsync(SegmentWrap segmentWrap)
        {
            if (segmentWrap == null)
            {
                throw new ArgumentNullException(nameof(segmentWrap));
            }

            segmentWrap.Description = segmentWrap.Description?.Trim();
            segmentWrap.Name = segmentWrap.Name?.Trim();
            segmentWrap.Prefix = segmentWrap.Prefix?.Trim();
            segmentWrap.Suffix = segmentWrap.Suffix?.Trim();
            await _segmentWrapRepository.AddAsync(segmentWrap);
            await _segmentWrapRepository.SaveAsync();
        }

        public async Task<bool> DisableAsync(int segmentWrapId)
        {
            var segmentsUsingWrap = await _segmentRepository.CountByWrapAsync(segmentWrapId);

            if (segmentsUsingWrap == 0)
            {
                await _segmentWrapRepository.PermanentlyDeleteAsync(segmentWrapId);
                return true;
            }
            else
            {
                var segmentWrap = await _segmentWrapRepository.FindAsync(segmentWrapId);
                if (segmentWrap == null)
                {
                    throw new OcudaException($"Unable to find Segment Wrap Id {segmentWrapId}");
                }
                segmentWrap.IsDeleted = true;
                _segmentWrapRepository.Update(segmentWrap);
                await _segmentWrapRepository.SaveAsync();
                return false;
            }
        }

        public async Task<SegmentWrap> FindAsync(int segmentWrapId)
        {
            return await _segmentWrapRepository.FindAsync(segmentWrapId);
        }

        public async Task<IDictionary<string, string>> GetActiveListAsync()
        {
            return await _segmentWrapRepository.GetActiveListAsync();
        }

        public async Task<CollectionWithCount<SegmentWrap>> GetPaginatedAsync(BaseFilter filter)
        {
            var segmentWrapList = await _segmentWrapRepository.GetPaginatedAsync(filter);

            foreach (var segmentWrap in segmentWrapList.Data)
            {
                segmentWrap.UsedByCount = await _segmentRepository.CountByWrapAsync(segmentWrap.Id);
            }
            return segmentWrapList;
        }

        public async Task UpdateAsync(SegmentWrap segmentWrap)
        {
            if (segmentWrap == null)
            {
                throw new ArgumentNullException(nameof(segmentWrap));
            }

            var dbSegmentWrap = await _segmentWrapRepository.FindAsync(segmentWrap.Id);
            if (dbSegmentWrap == null)
            {
                throw new OcudaException($"Unable to find Segment Wrap Id {segmentWrap.Id}");
            }

            dbSegmentWrap.Description = segmentWrap.Description?.Trim();
            dbSegmentWrap.Name = segmentWrap.Name?.Trim();
            dbSegmentWrap.Prefix = segmentWrap.Prefix?.Trim();
            dbSegmentWrap.Suffix = segmentWrap.Suffix?.Trim();

            _segmentWrapRepository.Update(dbSegmentWrap);
            await _segmentWrapRepository.SaveAsync();
        }
    }
}