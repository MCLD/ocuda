using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IImageFeatureItemTextRepository : IGenericRepository<ImageFeatureItemText>
    {
        Task<ImageFeatureItemText> GetByIdsAsync(int imageFeatureItemId, int languageId);
    }
}
