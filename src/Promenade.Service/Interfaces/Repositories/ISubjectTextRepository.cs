using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ISubjectTextRepository
    {
        Task<SubjectText> GetByIdsAsync(int subjectId, int languageId);
    }
}