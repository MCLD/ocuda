using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ISegmentWrapRepository : IGenericRepository<SegmentWrap>
    {
        public Task<SegmentWrap> GetActiveAsync(int segmentWrapId);
    }
}