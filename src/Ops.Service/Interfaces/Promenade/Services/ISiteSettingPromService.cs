using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ISiteSettingPromService
    {
        Task EnsureSiteSettingsExistAsync();

        Task<ICollection<SiteSetting>> GetAllAsync();

        Task<double> GetSettingDoubleAsync(string key);

        Task<int> GetSettingIntAsync(string key);

        Task<string> GetSettingStringAsync(string key);

        Task<SiteSetting> UpdateAsync(string key, string value);
    }
}
