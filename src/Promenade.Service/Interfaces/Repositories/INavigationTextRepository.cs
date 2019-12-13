using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface INavigationTextRepository
    {
        Task<NavigationText> FindAsync(int id, int languageId);
    }
}
