using System.Linq;
using System.Reflection;

namespace Ocuda.Utility.Helpers
{
    public static class VersionHelper
    {
        public static string GetVersion()
        {
            var thisAssemblyVersion = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
            var fileVersion = GetShortVersion();

            if (!string.IsNullOrEmpty(fileVersion)
                && fileVersion.Count(_ => _ == '.') > 2
                && fileVersion.Length > fileVersion.LastIndexOf('.'))
            {
                var revision = fileVersion.Substring(fileVersion.LastIndexOf('.') + 1);
                if (!string.IsNullOrEmpty(revision) && revision != "0")
                {
                    thisAssemblyVersion += "-" + revision;
                }
            }
            return thisAssemblyVersion;
        }

        public static string GetShortVersion()
        {
            return Assembly
                 .GetEntryAssembly()
                 .GetCustomAttribute<AssemblyFileVersionAttribute>()?
                 .Version;
        }
    }
}
