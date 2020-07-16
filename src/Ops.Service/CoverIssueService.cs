using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service
{
    public class CoverIssueService : BaseService<CoverIssueService>, ICoverIssueService
    {
        private readonly ICoverIssueHeaderRepository _coverIssueHeaderRepository;
        private readonly ICoverIssueDetailRepository _coverIssueDetailRepository;

        public CoverIssueService(ILogger<CoverIssueService> logger,
            IHttpContextAccessor httpContextAccessor,
            ICoverIssueHeaderRepository coverIssueHeaderRepository,
            ICoverIssueDetailRepository coverIssueDetailRepository)
            : base(logger, httpContextAccessor)
        {
            _coverIssueHeaderRepository = coverIssueHeaderRepository
                ?? throw new ArgumentNullException(nameof(coverIssueHeaderRepository));
            _coverIssueDetailRepository = coverIssueDetailRepository
                ?? throw new ArgumentNullException(nameof(coverIssueDetailRepository));
        }

        public async Task<DataWithCount<ICollection<CoverIssueHeader>>> GetPaginatedHeaderListAsync(
            CoverIssueFilter filter)
        {
            return await _coverIssueHeaderRepository.GetPaginiatedListAsync(filter);
        }

        public async Task<CoverIssueHeader> GetHeaderByIdAsync(int id)
        {
            return await _coverIssueHeaderRepository.FindAsync(id);
        }

        public async Task<ICollection<CoverIssueDetail>> GetDetailsByHeaderIdAsync(int headerId)
        {
            return await _coverIssueDetailRepository.GetByHeaderIdAsync(headerId,
                includeCreatedByUser: true);
        }

        public async Task AddCoverIssueAsync(int bibId)
        {
            var now = DateTime.Now;
            var currentUserId = GetCurrentUserId();

            var header = await _coverIssueHeaderRepository.GetByBibIdAsync(bibId);
            if (header == null)
            {
                header = new CoverIssueHeader
                {
                    BibId = bibId,
                    CreatedAt = now,
                    CreatedBy = currentUserId,
                    HasPendingIssue = true
                };
                await _coverIssueHeaderRepository.AddAsync(header);
            }
            else
            {
                header.HasPendingIssue = true;
                _coverIssueHeaderRepository.Update(header);
            }

            var detail = new CoverIssueDetail
            {
                CoverIssueHeader = header,
                CreatedAt = now,
                CreatedBy = currentUserId
            };

            await _coverIssueDetailRepository.AddAsync(detail);
            await _coverIssueDetailRepository.SaveAsync();
        }

        public async Task ResolveCoverIssueAsnyc(int headerId)
        {
            var header = await _coverIssueHeaderRepository.FindAsync(headerId);

            var now = DateTime.Now;
            var currentUserId = GetCurrentUserId();

            header.HasPendingIssue = false;
            header.LastResolved = now;
            header.UpdatedAt = now;
            header.UpdatedBy = currentUserId;

            _coverIssueHeaderRepository.Update(header);

            var details = await _coverIssueDetailRepository.GetByHeaderIdAsync(header.Id,
                resolved: false);

            foreach (var detail in details)
            {
                detail.IsResolved = true;
                detail.UpdatedAt = now;
                detail.UpdatedBy = currentUserId;
            }
            _coverIssueDetailRepository.UpdateRange(details);

            await _coverIssueHeaderRepository.SaveAsync();
        }
    }
}
