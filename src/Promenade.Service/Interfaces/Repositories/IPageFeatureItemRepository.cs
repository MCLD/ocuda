using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPageFeatureItemRepository : IGenericRepository<PageFeatureItem>
    {
        Task<ICollection<PageFeatureItem>> GetActiveForPageFeatureAsync(int featureId);
    }
}
