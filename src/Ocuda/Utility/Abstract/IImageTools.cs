using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Utility.Models;

namespace Ocuda.Utility.Abstract
{
    public interface IImageTools
    {
        public Task<ImageInformation> GetInformationAsync(byte[] imageBytes);

        public Task<byte[]> GetLayeredImageAsync(IEnumerable<byte[]> layers, int x, int y);

        public Task<byte[]> GetResizedAsync(byte[] image, int width, int height);

        public Task<byte[]> GetCroppedAsync(byte[] image, int x, int y, int width, int height);
    }
}
