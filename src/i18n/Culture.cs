using System.Globalization;

namespace Ocuda.i18n
{
    public static class Culture
    {
        public static readonly string DefaultName = "en-US";
        public static readonly string EnglishUS = "en-US";
        public static readonly string EspanolUS = "es-US";

        public static readonly CultureInfo[] SupportedCultures = new[]
        {
            DefaultCulture,
            new CultureInfo(EspanolUS)
        };

        private static CultureInfo _defaultCulture;

        public static CultureInfo DefaultCulture
        {
            get
            {
                return _defaultCulture ??= new(DefaultName);
            }
        }
    }
}