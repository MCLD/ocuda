using System;
using System.Collections.Generic;

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
    }
}