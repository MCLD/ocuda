using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class CoverIssueService : ICoverIssueService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly ISiteSettingService _siteSettingService;
        private readonly IUserService _userService;
        private readonly ICoverIssueTypeRepository _coverIssueTypeRepository;
        private readonly ICoverIssueHeaderRepository _coverIssueHeaderRepository;
        private readonly ICoverIssueDetailRepository _coverIssueDetailRepository;

        public CoverIssueService(ILogger<EmailService> logger,
           ISiteSettingService siteSettingService,
           IUserService userService,
           ICoverIssueTypeRepository coverIssueTypeRepository,
           ICoverIssueHeaderRepository coverIssueHeaderRepository,
           ICoverIssueDetailRepository coverIssueDetailRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _coverIssueTypeRepository = coverIssueTypeRepository
                ?? throw new ArgumentNullException(nameof(coverIssueTypeRepository));
            _coverIssueHeaderRepository = coverIssueHeaderRepository
                ?? throw new ArgumentNullException(nameof(coverIssueHeaderRepository));
            _coverIssueDetailRepository = coverIssueDetailRepository
                ?? throw new ArgumentNullException(nameof(coverIssueDetailRepository));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<CoverIssueType> AddNewCoverIssueTypeAsync(CoverIssueType issueType) {
            issueType.Name = issueType.Name.Trim();
            await ValidateCoverIssueTypeAsync(issueType);
            await _coverIssueTypeRepository.AddAsync(issueType);
            await _coverIssueTypeRepository.SaveAsync();
            return issueType;
        }

        public async Task<CoverIssueHeader> AddNewCoverIssueHeaderAsync(CoverIssueHeader issueHeader)
        {
            await _coverIssueHeaderRepository.AddAsync(issueHeader);
            await _coverIssueHeaderRepository.SaveAsync();
            return issueHeader;
        }

        public async Task<CoverIssueDetail> AddNewCoverIssueDetailAsync(CoverIssueDetail issueDetail)
        {
            issueDetail.Message = issueDetail.Message.Trim();
            await _coverIssueDetailRepository.AddAsync(issueDetail);
            await _coverIssueDetailRepository.SaveAsync();
            return issueDetail;
        }

        public async Task<List<CoverIssueType>> GetAllCoverIssueTypesAsync()
        {
            return await _coverIssueTypeRepository.GetAllTypesAsync();
        }

        public async Task<List<CoverIssueHeader>> GetAllCoverIssueHeadersAsync()
        {
            return await _coverIssueHeaderRepository.GetAllHeadersAsync();
        }

        public async Task<DataWithCount<ICollection<CoverIssueHeader>>> PageHeaderItemsAsync(
            CoverIssueHeaderFilter filter)
        {
            return new DataWithCount<ICollection<CoverIssueHeader>>
            {
                Data = await _coverIssueHeaderRepository.PageAsync(filter),
                Count = await _coverIssueHeaderRepository.CountAsync(filter)
            };
        }

        public CoverIssueHeader GetCoverIssueHeaderByBibId(int bibId)
        {
            return _coverIssueHeaderRepository.GetCoverIssueHeaderByBibID(bibId);
        }

        public async Task<CoverIssueHeader> GetCoverIssueHeaderByDetailIdAsync(int detailId)
        {
            var detail = await _coverIssueDetailRepository.FindAsync(detailId);
            return await _coverIssueHeaderRepository.FindAsync(detail.CoverIssueHeaderId);
        }

        public async Task<List<CoverIssueDetail>> GetCoverIssueDetailsByHeaderAsync(int headerId)
        {
            var details = await _coverIssueDetailRepository.GetCoverIssueDetailsByHeader(headerId);
            foreach (var item in details)
            {
                item.CreatedByUser = await _userService.GetByIdAsync(item.CreatedBy);
            }
            return details;
        }

        public async Task<CoverIssueDetail> GetCoverIssueDetailByIdAsync(int detailId)
        {
            return await _coverIssueDetailRepository.FindAsync(detailId);
        }

        public async Task<CoverIssueType> GetCoverIssueTypeByIdAsync(int typeId)
        {
            return await _coverIssueTypeRepository.FindAsync(typeId);
        }

        public async Task CreateNewCoverIssue(CoverIssueDetail detail, CoverIssueHeader header)
        {
            var issueHeader = _coverIssueHeaderRepository.GetCoverIssueHeaderByBibID(header.BibID);
            if (issueHeader == null)
            {
                issueHeader = new CoverIssueHeader
                {
                    BibID = header.BibID,
                    CreatedAt = header.CreatedAt,
                    CreatedBy = header.CreatedBy,
                    HasIssue = true
                };
                await _coverIssueHeaderRepository.AddAsync(issueHeader);
                await _coverIssueHeaderRepository.SaveAsync();
                issueHeader = _coverIssueHeaderRepository.GetCoverIssueHeaderByBibID(header.BibID);
            }
            else
            {
                issueHeader.HasIssue = true;
                _coverIssueHeaderRepository.Update(issueHeader);
                await _coverIssueHeaderRepository.SaveAsync();
            }
            detail.Message = detail.Message?.Trim();
            detail.Isbn = detail.Isbn.Trim();
            detail.UPC = detail.UPC?.Trim();
            detail.OCLC = detail.OCLC?.Trim();
            detail.IsOpenIssue = true;
            detail.CoverIssueHeaderId = issueHeader.Id;
            await _coverIssueDetailRepository.AddAsync(detail);
            await _coverIssueDetailRepository.SaveAsync();
        }

        public async Task ResolveCoverIssue(int detailId)
        {
            var detail = await _coverIssueDetailRepository.FindAsync(detailId);
            detail.IsOpenIssue = false;
            _coverIssueDetailRepository.Update(detail);
            await _coverIssueDetailRepository.SaveAsync();
            var duplicates = await _coverIssueDetailRepository
               .GetCoverIssueDetailsByHeader(detail.CoverIssueHeaderId);
            var type = await _coverIssueTypeRepository.FindAsync(detail.CoverIssueTypeId);
            if (!type.HasMessage)
            {
                foreach (var dupe in duplicates.Where(_ => _.IsOpenIssue
                    && _.CoverIssueTypeId == type.Id))
                {
                    dupe.IsOpenIssue = false;
                    _coverIssueDetailRepository.Update(dupe);
                    await _coverIssueDetailRepository.SaveAsync();
                }
            }
            if (!duplicates.Any(_ => _.IsOpenIssue))
            {
                var header = await _coverIssueHeaderRepository
                    .FindAsync(detail.CoverIssueHeaderId);
                header.HasIssue = false;
                header.LastResolved = DateTime.Now;
                _coverIssueHeaderRepository.Update(header);
                await _coverIssueHeaderRepository.SaveAsync();
            }
        }

        private async Task ValidateCoverIssueTypeAsync(CoverIssueType issueType)
        {
            if (await _coverIssueTypeRepository.IsDuplicateName(issueType))
            {
                throw new OcudaException($"Cover Issue Type '{issueType.Name}' already exists.");
            }
        }
    }
}
