using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ISubjectTextRepository : IGenericRepository<SubjectText>
    {
        Task<ICollection<SubjectText>> GetAllForSubjectAsync(int subjectId);

        Task<SubjectText> GetBySubjectAndLanguageAsync(int subjectId, int languageId);

        Task<ICollection<string>> GetUsedLanguagesForSubjectAsync(int subjectId);
    }
}