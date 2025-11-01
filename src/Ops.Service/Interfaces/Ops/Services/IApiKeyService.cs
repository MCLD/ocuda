using System;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IApiKeyService
    {
        Task<string> CreateAsync(ApiKeyType keyType, int representsUserId, DateTime? endDate);

        Task DeleteAsync(int apiKeyId);

        Task<ApiKey> FindAsync(string apiKey);

        Task<CollectionWithCount<ApiKey>> PageAsync(BaseFilter filter);
    }
}