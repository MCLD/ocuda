namespace Ocuda.Utility.Extensions
{
    public static class FileInfoExtensions
    {
        /// <summary>
        /// <para>Extension method for showing file sizes in a human-readable format.</para>
        /// <para>Found at https://www.somacon.com/p576.php</para>
        /// <para>
        /// License: dedicated to the public domain under the Unlicense or CC0, whichever is most
        /// appropriate for your use
        /// </para>
        /// </summary>
        /// <param name="fileInfo">File object that is extended</param>
        /// <param name="fractional">
        /// Whether not to display three decimal places in the file size
        /// </param>
        /// <returns>Human-readable file size (e.g. 5.023 KB)</returns>
        public static string HumanSize(this System.IO.FileInfo fileInfo, bool fractional)
        {
            var fileLength = fileInfo.Length;
            // Get absolute value
            var absoluteLength = fileLength < 0 ? -fileLength : fileLength;
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absoluteLength >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (fileLength >> 50);
            }
            else if (absoluteLength >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (fileLength >> 40);
            }
            else if (absoluteLength >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (fileLength >> 30);
            }
            else if (absoluteLength >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (fileLength >> 20);
            }
            else if (absoluteLength >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (fileLength >> 10);
            }
            else if (absoluteLength >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = fileLength;
            }
            else
            {
                return $"{fileLength} B"; // Byte
            }

            // Divide by 1024
            readable /= 1024;

            if (fractional)
            {
                return $"{readable:0.###} {suffix}";
            }
            else
            {
                return $"{readable:0} {suffix}";
            }
        }

        /// <summary>
        /// <para>Extension method for showing file sizes in a human-readable format.</para>
        /// <para>Found at https://www.somacon.com/p576.php</para>
        /// <para>
        /// License: dedicated to the public domain under the Unlicense or CC0, whichever is most
        /// appropriate for your use
        /// </para>
        /// </summary>
        /// <param name="fileInfo">File object that is extended</param>
        /// <returns>Human-readable file size (e.g. 5 KB)</returns>
        public static string HumanSize(this System.IO.FileInfo fileInfo)
        {
            return HumanSize(fileInfo, false);
        }
    }
}