using System.Globalization;

namespace Ocuda.Utility.Helpers
{
    public class TextFormattingHelper
    {
        public static string FormatPhone(string tenDigits)
        {
            if (string.IsNullOrWhiteSpace(tenDigits) || tenDigits.Length < 10)
            {
                return tenDigits;
            }
            return string.Format(CultureInfo.InvariantCulture,
                        "{0}-{1}-{2}",
                        tenDigits.Substring(0, 3),
                        tenDigits.Substring(3, 3),
                        tenDigits.Substring(6, 4));
        }
    }
}