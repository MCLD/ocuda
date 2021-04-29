using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPageFeatureItemTextRepository : IGenericRepository<PageFeatureItemText>
    {
        Task<PageFeatureItemText> GetByIdsAsync(int featureItemId, int languageId);
    }
}
