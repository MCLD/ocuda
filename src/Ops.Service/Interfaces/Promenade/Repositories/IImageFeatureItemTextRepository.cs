using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IImageFeatureItemTextRepository : IGenericRepository<ImageFeatureItemText>
    {
        void DetachEntity(ImageFeatureItemText itemText);

        Task<ICollection<ImageFeatureItemText>> GetAllForImageFeatureItemAsync(int itemId);

        Task<ImageFeatureItemText> GetByImageFeatureItemAndLanguageAsync(int imageFeatureItemId,
            int languageId);
    }
}