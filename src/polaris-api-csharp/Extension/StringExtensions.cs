using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clc.Polaris.Api
{
    /// <summary>
    /// String helper extension methods
    /// </summary>
	public static class StringExtensions
	{
        /// <summary>
        /// Check if the string has a value or not
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool HasValue(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }
	}
}
