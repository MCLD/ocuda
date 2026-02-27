using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ISubjectService
    {
        Task<Subject> CreateAsync(Subject subject);

        Task DeleteAsync(int id);

        Task<Subject> EditAsync(Subject subject);

        Task<ICollection<Subject>> GetAllAsync();

        Task<Subject> GetByIdAsync(int id);

        Task<DataWithCount<ICollection<Subject>>> GetPaginatedListAsync(BaseFilter filter);

        Task<ICollection<string>> GetSubjectEmediasAsync(int id);

        Task<ICollection<string>> GetSubjectLanguagesAsync(int id);

        Task<SubjectText> GetTextBySubjectAndLanguageAsync(int subjectId, int languageId);

        Task SetSubjectTextAsync(SubjectText subjectText);
    }
}