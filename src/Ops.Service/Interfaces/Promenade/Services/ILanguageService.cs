using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ILanguageService
    {
        Task<ICollection<Language>> GetActiveAsync();
        Task<Language> GetActiveByIdAsync(int id);
    }
}
