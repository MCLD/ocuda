using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ICoverIssueService
    {
        Task<CoverIssueType> AddNewCoverIssueTypeAsync(CoverIssueType issueType);

        Task<CoverIssueHeader> AddNewCoverIssueHeaderAsync(CoverIssueHeader issueHeader);

        Task<CoverIssueDetail> AddNewCoverIssueDetailAsync(CoverIssueDetail issueDetail);

        Task<List<CoverIssueType>> GetAllCoverIssueTypesAsync();

        Task CreateNewCoverIssue(CoverIssueDetail detail, CoverIssueHeader header);

        Task<List<CoverIssueHeader>> GetAllCoverIssueHeadersAsync();

        Task<DataWithCount<ICollection<CoverIssueHeader>>> PageHeaderItemsAsync(
            CoverIssueHeaderFilter filter);

        CoverIssueHeader GetCoverIssueHeaderByBibId(int bibId);

        Task<List<CoverIssueDetail>> GetCoverIssueDetailsByHeaderAsync(int headerId);
        Task ResolveCoverIssue(int detailId);
        Task<CoverIssueHeader> GetCoverIssueHeaderByDetailIdAsync(int detailId);

        Task<CoverIssueDetail> GetCoverIssueDetailByIdAsync(int detailId);

        Task<CoverIssueType> GetCoverIssueTypeByIdAsync(int typeId);
    }
}
