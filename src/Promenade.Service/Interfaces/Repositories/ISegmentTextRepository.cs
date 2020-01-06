
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ISegmentTextRepository : IGenericRepository<SegmentText, int>
    {
        public SegmentText GetSegmentTextBySgmentId(int segmentId);
    }
}
