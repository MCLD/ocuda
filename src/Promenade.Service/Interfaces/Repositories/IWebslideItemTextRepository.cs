using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IWebslideItemTextRepository : IGenericRepository<WebslideItemText>
    {
        Task<WebslideItemText> GetByIdsAsync(int webslideItemId, int languageId);
    }
}
