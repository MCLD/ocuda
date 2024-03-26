using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IImageAltTextRepository : IGenericRepository<ImageAltText>
    {
        Task<ImageAltText> GetImageAltTextAsync(int imageId, int languageId);
        Task<List<ImageAltText>> GetAllLanguageImageAltTextsAsync(int imageId);
    }
}