using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICoverIssueTypeRepository : IRepository<CoverIssueType, int>
    {
        Task<bool> IsDuplicateName(CoverIssueType issueType);
        Task<List<CoverIssueType>> GetAllTypesAsync();
    }
}