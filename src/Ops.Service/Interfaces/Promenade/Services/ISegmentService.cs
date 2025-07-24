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

        /// <summary>
        /// Delete a Segment and any related SegmentTexts after validating that they are not in use
        /// by:
        /// - Emedia Groups
        /// - Locations
        /// - Schedule Request Subjects
        /// - Podcast episode notes
        /// </summary>
        /// <param name="segmentId">Segment id</param>
        Task DeleteAsync(int segmentId);

        Task DeleteNoSaveAsync(int id);

        Task DeleteSegmentTextAsync(SegmentText segmentText);

        /// <summary>
        /// Delete a Segment and any related SegmentTexts with NO in-use validation. Either verify
        /// that the segment is no longer needed prior to calling or use DeleteAsync().
        /// <param name="segmentId">Segment id</param>
        Task DeleteWithTextsAlreadyVerifiedAsync(int segmentId);

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