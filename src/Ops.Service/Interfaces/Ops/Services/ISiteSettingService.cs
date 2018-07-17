using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ISiteSettingService
    {
        Task<ICollection<SiteSetting>> GetAllAsync();
        Task<string> GetSetting(string key);
        Task<SiteSetting> UpdateAsync(string key, string value);
    }
}
