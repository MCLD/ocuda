using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ISegmentTextRepository : IRepository<SegmentText, int>
    {
        SegmentText GetSegmentTextBySegmentId(int segmentId, int languageId);

        Task<List<string>> GetUsedLanguageNamesBySegmentId(int segmentId);
    }
}
