using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Utility.Helpers
{
    public static class FileHelper
    {
        public static string GetUniqueFilename(string path, string filename)
        {
            int renameCounter = 1;
            while (File.Exists(Path.Combine(path, filename)))
            {
                filename = string.Format(CultureInfo.InvariantCulture,
                    "{0}-{1}{2}",
                    Path.GetFileNameWithoutExtension(filename),
                    renameCounter++,
                    Path.GetExtension(filename));

                if (renameCounter > 9999)
                {
                    throw new OcudaException($"Unable to create valid filename for {filename} in {path}");
                }
            }
            return filename;
        }

        public static string MakeValidFilename(string text, char? replacement)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var sb = new StringBuilder(text.Length);
            bool changed = false;
            foreach (char c in text)
            {
                if (Path.GetInvalidFileNameChars().Contains(c))
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