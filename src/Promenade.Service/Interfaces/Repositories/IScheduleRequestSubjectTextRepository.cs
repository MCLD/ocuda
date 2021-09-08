using System.Threading.Tasks;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IScheduleRequestSubjectTextRepository
    {
        Task<string> GetByIdsAsync(int scheduleRequestSubjectId, int languageId);
    }
}