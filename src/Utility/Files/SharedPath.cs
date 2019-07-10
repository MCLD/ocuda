using System.IO;

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
                path = Path.Combine(Directory.GetCurrentDirectory(), DefaultSharedDirectoryPath);
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
