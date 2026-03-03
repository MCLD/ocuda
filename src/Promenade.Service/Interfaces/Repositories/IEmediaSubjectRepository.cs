using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IEmediaSubjectRepository : IGenericRepository<EmediaSubject>
    {
        public Task<int[]> GetEmediaIdsAsync(int subjectId);
    }
}