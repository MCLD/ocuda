using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IDigitalDisplayCleanupService
    {
        public Task CleanupSlidesAsync();
    }
}