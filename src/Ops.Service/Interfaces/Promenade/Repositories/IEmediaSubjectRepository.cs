using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaSubjectRepository : IGenericRepository<EmediaSubject>
    {
        Task<ICollection<EmediaSubject>> GetAllForEmediaAsync(int emediaId);

        Task<ICollection<EmediaSubject>> GetAllForGroupAsync(int groupId);

        Task<ICollection<EmediaSubject>> GetBySubjectIdAsync(int subjectId);

        Task<ICollection<string>> GetEmediasForSubjectAsync(int subjectId);

        Task<ICollection<int>> GetSubjectIdsForEmediaAsync(int emediaId);

        Task<ICollection<Subject>> GetSubjectsForEmediaAsync(int emediaId);

        void RemoveByEmediaAndSubjects(int emediaId, ICollection<int> subjectIds);
    }
}