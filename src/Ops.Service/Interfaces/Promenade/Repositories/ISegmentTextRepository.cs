using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ISegmentTextRepository : IGenericRepository<SegmentText>
    {
        Task<ICollection<SegmentText>> GetBySegmentIdAsync(int segmentId);
        Task<SegmentText> GetBySegmentAndLanguageAsync(int segmentId, int languageId);
        Task<List<string>> GetUsedLanguageNamesBySegmentId(int segmentId);
    }
}
