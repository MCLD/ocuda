using System.Globalization;
using System.IO;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Utility.Helpers
{
    public static class FileHelper
    {
        public static string GetUniqueFilename(string path, string filename)
        {
            string baseFilename = filename;
            int renameCounter = 1;

            while (File.Exists(Path.Combine(path, filename)))
            {
                filename = string.Format(CultureInfo.InvariantCulture,
                    "{0}-{1}{2}",
                    Path.GetFileNameWithoutExtension(baseFilename),
                    renameCounter++,
                    Path.GetExtension(baseFilename));

                if (renameCounter > 999)
                {
                    throw new OcudaException(
                        $"Unable to find a valid filename for {filename} in {path} after {renameCounter} tries");
                }
            }

            return filename;
        }

        public static string MakeValidFilename(string text, char replacement)
        {
            return string.IsNullOrEmpty(text)
                ? null
                : string.Join(replacement, text.Split(Path.GetInvalidFileNameChars())).Trim();
        }

        public static string MakeValidFilename(string text)
        {
            return MakeValidFilename(text, '_');
        }
    }
}
