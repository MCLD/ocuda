using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ICarouselButtonLabelTextRepository
        : IGenericRepository<CarouselButtonLabelText>
    {
        Task<CarouselButtonLabelText> GetByIdsAsync(int labelId, int languageId);
    }
}
