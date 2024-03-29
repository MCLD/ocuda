using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IImageAltTextRepository : IGenericRepository<ImageAltText>
    {
        Task<ImageAltText> GetByImageIdAsync(int imageId, int languageId);
    }
}