using System.Reflection;

namespace Ocuda.Utility.Helpers
{
    public static class VersionHelper
    {
        public static string GetVersion()
        {
            var fileVersion = Assembly
                 .GetEntryAssembly()
                 .GetCustomAttribute<AssemblyFileVersionAttribute>()?
                 .Version;

            return !string.IsNullOrEmpty(fileVersion)
                ? fileVersion
                : Assembly.GetEntryAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    .InformationalVersion;
        }
    }
}