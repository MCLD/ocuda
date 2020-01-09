
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ISegmentTextRepository : IGenericRepository<SegmentText, int>
    {
        public SegmentText GetSegmentTextBySegmentId(int segmentId);
        SegmentText GetSegmentTextBySgmentAndLanguageId(int segmentId, int languageId);
    }
}
