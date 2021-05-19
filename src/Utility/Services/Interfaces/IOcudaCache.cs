using System;
using System.Threading.Tasks;

namespace Ocuda.Utility.Services.Interfaces
{
    public interface IOcudaCache
    {
        Task<bool?> GetBoolFromCacheAsync(string cacheKey);

        Task<int?> GetIntFromCacheAsync(string cacheKey);

        Task<T> GetObjectFromCacheAsync<T>(string cacheKey) where T : class;

        Task<string> GetStringFromCache(string cacheKey);

        Task RemoveAsync(string cacheKey);

        Task SaveToCacheAsync<T>(string cacheKey, T item, int cacheForHours);

        Task SaveToCacheAsync<T>(string cacheKey, T item, TimeSpan absoluteExpiration);

        Task SaveToCacheAsync<T>(string cacheKey,
            T item,
            TimeSpan? absoluteExpiration,
            TimeSpan slidingExpiration);
    }
}