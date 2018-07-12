using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ocuda.Utility.File
{
    public class SharedPath
    {
        private const string DefaultSharedDirectoryPath = "shared";

        public static string Get(string configuredSharedPath)
        {
            string path = null;
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
