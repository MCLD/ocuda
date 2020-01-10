using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILanguageRepository : IGenericRepository<Language>
    {
        Task Add(Language language);
        void Update(Language language);
        Task SaveAsync();
        Task<ICollection<Language>> GetAllAsync();
        Task<ICollection<Language>> GetActiveAsync();
        Task<Language> GetActiveByIdAsync(int id);
        Task<int> GetDefaultLanguageId();
        Task<int> GetLanguageId(string culture);
    }
}
