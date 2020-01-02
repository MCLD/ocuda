using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IInitialSetupService
    {
        Task VerifyInitialSetupAsync();
    }
}
