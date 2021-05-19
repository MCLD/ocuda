using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IImageFeatureItemRepository : IGenericRepository<ImageFeatureItem>
    {
        Task<ImageFeatureItem> FindAsync(int id);

        Task<ImageFeatureItem> GetByImageFeatureAndOrderAsync(int imageFeatureId, int order);

        Task<ICollection<ImageFeatureItem>> GetByImageFeatureAsync(int imageFeatureId);

        Task<List<ImageFeatureItem>> GetImageFeatureSubsequentAsync(int imageFeatureId, int order);

        Task<int?> GetMaxSortOrderForImageFeatureAsync(int imageFeatureId);
    }
}