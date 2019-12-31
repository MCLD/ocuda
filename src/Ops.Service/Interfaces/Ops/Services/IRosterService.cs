using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IRosterService
    {
        Task<int> ImportRosterAsync(int currentUserId, string filename);
    }
}
