using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface ISectionRepository : IRepository<Section, int>
    {
        Task<Section> GetDefaultSectionAsync();
    }
}
