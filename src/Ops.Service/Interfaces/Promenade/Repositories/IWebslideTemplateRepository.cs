using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IWebslideTemplateRepository : IGenericRepository<WebslideTemplate>
    {
        Task<ICollection<WebslideTemplate>> GetAllAsync();
        Task<WebslideTemplate> GetForPageLayoutAsync(int pageLayoutId);
        Task<WebslideTemplate> GetForWebslideAsync(int webslideId);
    }
}
