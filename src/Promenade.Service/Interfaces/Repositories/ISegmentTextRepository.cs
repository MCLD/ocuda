
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ISegmentTextRepository : IGenericRepository<SegmentText, int>
    {
        SegmentText GetSegmentTextBySegmentAndLanguageId(int segmentId, int languageId);
    }
}
