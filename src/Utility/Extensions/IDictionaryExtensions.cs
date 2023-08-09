using System.Collections.Generic;
using System.Linq;

namespace Ocuda.Utility.Extensions
{
    public static class IDictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
            IDictionary<TKey, TValue> items)
        {
            items.ToList().ForEach(_ => dictionary.Add(_.Key, _.Value));
        }
    }
}