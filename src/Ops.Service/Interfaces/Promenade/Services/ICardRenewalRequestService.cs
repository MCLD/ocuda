using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ICardRenewalRequestService
    {
        Task<CardRenewalRequest> GetRequestAsync(int id);
        Task<int> GetRequestCountAsync(bool? isProcessed);
        Task<CollectionWithCount<CardRenewalRequest>> GetRequestsAsync(RequestFilter filter);
    }
}
