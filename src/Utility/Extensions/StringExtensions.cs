using System;

namespace Ocuda.Utility.Extensions
{
    public static class StringExtensions
    {
        public static string TruncateTo(this string value, int length) =>
            value?[0..Math.Min(value.Length, length)];
    }
}