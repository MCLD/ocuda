using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ocuda.Utility.Helpers
{
    public static class FormFileHelper
    {
        public static async Task<byte[]> GetFileBytesAsync(IFormFile file)
        {
            if (file == null)
            {
                return null;
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
