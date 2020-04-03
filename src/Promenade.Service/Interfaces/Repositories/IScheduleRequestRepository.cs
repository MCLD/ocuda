using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IScheduleRequestRepository : IGenericRepository<ScheduleRequest>
    {
        public Task<ScheduleRequest> AddSaveAsync(ScheduleRequest scheduleRequest);
    }
}
