using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ICardRenewalRequestRepository : IGenericRepository<CardRenewalRequest>
    {
        Task AddSaveAsync(CardRenewalRequest request);
        Task<CardRenewalRequest> GetPendingRequestAsync(int patronId);
    }
}
