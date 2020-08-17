using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ISegmentService
    {
        Task<DataWithCount<ICollection<Segment>>> GetPaginatedListAsync(
            BaseFilter filter);

        Task<ICollection<Segment>> GetActiveSegmentsAsync();
        Task<ICollection<string>> GetSegmentLanguagesByIdAsync(int id);
        Task<Segment> GetByIdAsync(int segmentId);
        Task<SegmentText> GetBySegmentAndLanguageAsync(int segmentId, int languageId);
        Task<Segment> CreateAsync(Segment segment);
        Task<Segment> EditAsync(Segment segment);
        Task DeleteAsync(int id);
        Task CreateSegmentTextAsync(SegmentText segmentText);
        Task EditSegmentTextAsync(SegmentText segmentText);
        Task DeleteSegmentTextAsync(SegmentText segmentText);
    }
}
