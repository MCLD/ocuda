﻿using System.Globalization;

namespace Ocuda.i18n
{
    public static class Culture
    {
        public static readonly string EnglishUS = "en-US";
        public static readonly string EspanolUS = "es-US";

        public static readonly string DefaultName = EnglishUS;
        public static readonly CultureInfo DefaultCulture = new CultureInfo(DefaultName);

        public static readonly CultureInfo[] SupportedCultures = new[]
        {
            new CultureInfo(EnglishUS),
            //new CultureInfo(EspanolUS)
        };
    }
}
