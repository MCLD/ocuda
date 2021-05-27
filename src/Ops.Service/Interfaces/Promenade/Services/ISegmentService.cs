using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ISegmentService
    {
        Task<Segment> CreateAsync(Segment segment);

        Task<Segment> CreateNoSaveAsync(Segment segment);

        Task CreateSegmentTextAsync(SegmentText segmentText);

        Task DeleteAsync(int id);

        Task DeleteNoSaveAsync(int id);

        Task DeleteSegmentTextAsync(SegmentText segmentText);

        Task<Segment> EditAsync(Segment segment);

        Task EditSegmentTextAsync(SegmentText segmentText);

        Task<ICollection<Segment>> GetActiveSegmentsAsync();

        Task<Segment> GetByIdAsync(int segmentId);

        Task<SegmentText> GetBySegmentAndLanguageAsync(int segmentId, int languageId);

        Task<IDictionary<int, string>> GetNamesByIdsAsync(IEnumerable<int> ids);

        Task<int?> GetPageHeaderIdForSegmentAsync(int id);

        Task<int?> GetPageLayoutIdForSegmentAsync(int id);

        Task<DataWithCount<ICollection<Segment>>> GetPaginatedListAsync(BaseFilter filter);
        Task<ICollection<string>> GetSegmentLanguagesByIdAsync(int id);
    }
}
