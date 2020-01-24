using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IEmediaTextRepository
    {
        Task<EmediaText> GetByIdsAsync(int emediaId, int languageId);
    }
}
