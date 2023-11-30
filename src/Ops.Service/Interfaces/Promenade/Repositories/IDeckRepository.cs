using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IDeckRepository : IGenericRepository<Deck>
    {
        Task DeleteDeckAsync(int deckId);

        Task FixOrderAsync(int deckId);

        Task<Deck> GetByIdAsync(int deckId);

        Task<int?> GetPageHeaderIdAsync(int deckId);

        Task<int?> GetPageLayoutIdAsync(int deckId);
    }
}