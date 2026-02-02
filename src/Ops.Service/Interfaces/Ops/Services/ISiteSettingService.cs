using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ISiteSettingService
    {
        Task EnsureSiteSettingsExistAsync(int sysadminId);
        Task<ICollection<SiteSetting>> GetAllAsync();
        Task<bool> GetSettingBoolAsync(string key);
        Task<double> GetSettingDoubleAsync(string key);
        Task<int> GetSettingIntAsync(string key);
        Task<string> GetSettingStringAsync(string key);
        Task<SiteSetting> UpdateAsync(string key, string value);
    }
}
