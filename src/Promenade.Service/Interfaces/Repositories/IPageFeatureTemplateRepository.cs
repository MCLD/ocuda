using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPageFeatureTemplateRepository : IGenericRepository<PageFeatureTemplate>
    {
        Task<PageFeatureTemplate> GetForPageLayoutAsync(int pageLayoutId);
    }
}
