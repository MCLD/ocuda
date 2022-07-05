using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Ocuda.Utility.Helpers
{
    public static class FileHelper
    {
        private static char[] _invalids;

        public static string MakeValidFilename(string text, char? replacement)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var sb = new StringBuilder(text.Length);
            var invalids = _invalids ??= Path.GetInvalidFileNameChars();
            bool changed = false;
            foreach (char c in text)
            {
                if (invalids.Contains(c))
                {
                    changed = true;
                    if (replacement.HasValue)
                    {
                        sb.Append(replacement);
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length == 0)
            {
                return "_";
            }

            return changed ? sb.ToString() : text;
        }

        public static string MakeValidFilename(string text)
        {
            return MakeValidFilename(text, '_');
        }
    }
}
