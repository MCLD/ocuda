using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface ISiteSettingRepository : IRepository<SiteSetting, int>
    {
        Task<SiteSetting> FindByKeyAsync(string key);
    }
}
