using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Utility.Extensions
{
    public static class ICollectionExtension
    {
        public static void AddRange<T>(this ICollection<T> list, ICollection<T> items)
        {
            ArgumentNullException.ThrowIfNull(list);
            ArgumentNullException.ThrowIfNull(items);

            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        public static string HumanCommaList<T>(this ICollection<T> values)
        {
            StringBuilder builder = null;
            var count = values.Count;
            var current = 0;
            foreach (var value in values)
            {
                current++;
                if (builder == null)
                {
                    builder = new StringBuilder(value.ToString());
                }
                else
                {
                    builder.Append(current == count ? ", and " : ", ").Append(value);
                }
            }

            return builder.ToString();
        }
    }
}
