
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ISegmentTextRepository : IGenericRepository<SegmentText>
    {
        Task<SegmentText> FindAsync(int id);
        SegmentText GetSegmentTextBySegmentAndLanguageId(int segmentId, int languageId);
    }
}
