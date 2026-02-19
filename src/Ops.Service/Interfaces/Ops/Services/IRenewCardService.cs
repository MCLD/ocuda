using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Models.RenewCard;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IRenewCardService
    {
        Task<RenewCardResponse> CreateResponseAsync(RenewCardResponse response);
        Task DeleteResponseAsync(int id);
        Task DiscardRequestAsync(int id);
        Task<IEnumerable<RenewCardResponse>> GetAvailableResponsesAsync();
        Task<RenewCardResponse> GetResponseAsync(int id);
        Task<IEnumerable<RenewCardResponse>> GetResponsesAsync();
        Task<RenewCardResponse> GetResponseTextAsync(int responseId, int languageId);
        Task<RenewCardResult> GetResultForRequestAsync(int requestId);
        Task<bool> IsRequestAccepted(int requestId);
        Task<ProcessResult> ProcessRequestAsync(int requestId,
            int responseId,
            string responseText,
            string customerName);
        Task UpdateResponseAsync(RenewCardResponse response);
        Task UpdateResponseSortOrderAsync(int id, bool increase);
    }
}
