using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IScheduleRequestTelephoneRepository
        : IGenericRepository<ScheduleRequestTelephone>
    {
        public Task<ScheduleRequestTelephone> GetAsync(string phone);
        public Task<ScheduleRequestTelephone> AddSaveAsync(string phone);
    }
}
