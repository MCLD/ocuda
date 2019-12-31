using Microsoft.AspNetCore.Http;

namespace Ocuda.Utility.Helpers
{
    public static class IFormFileHelper
    {
        public static byte[] GetFileBytes(IFormFile file)
        {
            byte[] fileBytes;

            using (var fileStream = file.OpenReadStream())
            {
                using var ms = new System.IO.MemoryStream();
                fileStream.CopyTo(ms);
                fileBytes = ms.ToArray();
            }

            return fileBytes;
        }
    }
}
