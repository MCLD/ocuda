using System.Threading.Tasks;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IImageService
    {
        public string FormatMimeType { get; set; }

        public Task<ImageWithInformation> ConvertFromBase64Async(string imageBase64);

        public Task<ImageWithInformation> ConvertFromBase64Async(string imageBase64, bool isSquare);

        public Task<string> ConvertToBase64Async(byte[] imageBytes);

        public Task<ImageInformation> GetInformationAsync(byte[] imageBytes);

        public bool IsOptimizeConfigured();

        public Task<ImageWithInformation> OptimizeAsync(byte[] imageBytes, string filename);
    }
}
