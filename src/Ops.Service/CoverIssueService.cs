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
            BaseFilter filter)
        {
            return await _coverIssueHeaderRepository.GetPaginiatedListAsync(filter);
        }

        public async Task<CoverIssueHeader> GetHeaderByIdAsync(int id)
        {
            return await _coverIssueHeaderRepository.FindAsync(id);
        }

        public async Task<ICollection<CoverIssueDetail>> GetDetailsByHeaderIdAsync(int headerId)
        {
            return await _coverIssueDetailRepository.GetByHeaderIdAsync(headerId);
        }

        public async Task AddCoverIssueAsync(int bibId)
        {
            var header = await _coverIssueHeaderRepository.GetByBibIdAsync(bibId);
            if (header == null)
            {
                header = new CoverIssueHeader
                {
                    BibId = bibId,
                    CreatedAt = DateTime.Now,
                    CreatedBy = GetCurrentUserId(),
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
                CreatedAt = DateTime.Now,
                CreatedBy = GetCurrentUserId()
            };

            await _coverIssueDetailRepository.AddAsync(detail);
            await _coverIssueDetailRepository.SaveAsync();
        }

        public async Task ResolveCoverIssueAsnyc(int headerId)
        {
            var header = await _coverIssueHeaderRepository.FindAsync(headerId);
            header.HasPendingIssue = false;
            header.LastResolved = DateTime.Now;
            _coverIssueHeaderRepository.Update(header);

            var details = await _coverIssueDetailRepository.GetByHeaderIdAsync(header.Id, false);
            foreach (var detail in details)
            {
                detail.IsResolved = true;
            }
            _coverIssueDetailRepository.UpdateRange(details);

            await _coverIssueHeaderRepository.SaveAsync();
        }
    }
}
