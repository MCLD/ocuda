using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ISegmentService
    {
        Task<DataWithCount<ICollection<Segment>>> GetPaginatedListAsync(
            BaseFilter filter);

        Task<ICollection<string>> GetSegmentLanguagesByIdAsync(int id);
        Task<Segment> GetSegmentById(int segmentId);
        SegmentText GetBySegmentIdAndLanguage(int segmentId, int languageId);
        Task AddSegmentText(SegmentText segmentText);
        Task AddSegment(Segment segment);
        Segment FindSegmentByName(string name);
        Task EditSegment(Segment segment);
        Task EditSegmentText(SegmentText segmentText);
        Task DeleteSegmentText(SegmentText segmentText);
    }
}
