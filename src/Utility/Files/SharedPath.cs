using System.IO;
using Microsoft.Extensions.PlatformAbstractions;

namespace Ocuda.Utility.Files
{
    public static class SharedPath
    {
        private const string DefaultSharedDirectoryPath = "shared";

        public static string Get(string configuredSharedPath)
        {
            string path;
            if (!string.IsNullOrEmpty(configuredSharedPath))
            {
                path = configuredSharedPath;
            }
            else
            {
                path = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath,
                    DefaultSharedDirectoryPath);
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}