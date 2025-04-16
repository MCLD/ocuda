using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IScheduleLogCallDispositionRepository
        : IOpsRepository<ScheduleLogCallDisposition, int>
    {
        public Task<IEnumerable<ScheduleLogCallDisposition>> GetAllAsync();
    }
}