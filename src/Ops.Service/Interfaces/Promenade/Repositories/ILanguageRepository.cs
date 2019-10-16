using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ILanguageRepository : IRepository<Language, int>
    {
        Task<ICollection<Language>> GetAllAsync();
        Task<ICollection<Language>> GetActiveAsync();
        Task<Language> GetActiveByIdAsync(int id);
        Task<int> GetDefaultLanguageId();
        Task<int> GetLanguageId(string culture);
    }
}
