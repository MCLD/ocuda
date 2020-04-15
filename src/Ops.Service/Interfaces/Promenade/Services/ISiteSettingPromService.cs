using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ISiteSettingPromService
    {
        Task EnsureSiteSettingsExistAsync();
        Task<ICollection<SiteSetting>> GetAllAsync();
        Task<SiteSetting> UpdateAsync(string key, string value);
    }
}
