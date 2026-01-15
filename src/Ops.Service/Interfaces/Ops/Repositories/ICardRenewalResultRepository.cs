using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICardRenewalResultRepository : IOpsRepository<CardRenewalResult, int>
    {
        Task<CardRenewalResult> GetForRequestAsync(int requestId);
        Task<CardRenewalResponse.ResponseType> GetRequestResponseTypeAsync(int requestId);
    }
}
