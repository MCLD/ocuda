using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IImageFeatureTemplateRepository : IGenericRepository<ImageFeatureTemplate>
    {
        Task<ImageFeatureTemplate> GetForPageLayoutAsync(int pageLayoutId);
    }
}