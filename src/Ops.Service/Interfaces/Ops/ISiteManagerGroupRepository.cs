using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface ISiteManagerGroupRepository : IRepository<SiteManagerGroup, int>

    {
        Task<bool> IsSiteManagerAsync(string group);
    }
}
