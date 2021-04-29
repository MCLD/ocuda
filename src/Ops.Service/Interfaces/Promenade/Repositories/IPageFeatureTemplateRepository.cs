using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPageFeatureTemplateRepository : IGenericRepository<PageFeatureTemplate>
    {
        Task<ICollection<PageFeatureTemplate>> GetAllAsync();
        Task<PageFeatureTemplate> GetForPageFeatureAsync(int featureId);
    }
}
