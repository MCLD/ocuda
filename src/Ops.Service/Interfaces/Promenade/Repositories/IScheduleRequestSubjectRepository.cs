using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IScheduleRequestSubjectRepository : IGenericRepository<ScheduleRequestSubject>
    {
        Task<ICollection<ScheduleRequestSubject>> GetUsingSegmentAsync(int segmentId);
    }
}
