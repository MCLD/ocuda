using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Models.CardRenewal;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ICardRenewalService
    {
        Task<CardRenewalResponse> CreateResponseAsync(CardRenewalResponse response);
        Task DeleteResponseAsync(int id);
        Task DiscardRequestAsync(int id);
        Task<IEnumerable<CardRenewalResponse>> GetAvailableResponsesAsync();
        Task<CardRenewalResponse> GetResponseAsync(int id);
        Task<IEnumerable<CardRenewalResponse>> GetResponsesAsync();
        Task<CardRenewalResponse> GetResponseTextAsync(int responseId, int languageId);
        Task<CardRenewalResult> GetResultForRequestAsync(int requestId);
        Task<bool> IsRequestAccepted(int requestId);
        Task<ProcessResult> ProcessRequestAsync(int requestId,
            int responseId,
            string responseText,
            string customerName);
        Task UpdateResponseAsync(CardRenewalResponse response);
        Task UpdateResponseSortOrderAsync(int id, bool increase);
    }
}
